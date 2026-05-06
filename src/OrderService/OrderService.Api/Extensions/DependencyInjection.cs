using System.Reflection;
using Asp.Versioning;
using BuildingBlocks.HealthChecks;
using BuildingBlocks.Settings;
using MassTransit;
using Microsoft.Extensions.Options;
using OrderService.Application.Clients;
using OrderService.Application.Settings;
using OrderService.Infrastructure.Clients;
using OrderService.Persistence.Contexts;
using Polly;
using Polly.Extensions.Http;
using Refit;

namespace OrderService.Api.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Settings
        services.Configure<ProductServiceSettings>(configuration.GetSection(ProductServiceSettings.SectionName));
        services.AddOptions<WeatherApiSettings>()
            .Bind(configuration.GetSection(WeatherApiSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Versioning
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        // Health Checks - Extract connection string logic from Persistence layer
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "Your_strong_Password123";
        var dbName = Environment.GetEnvironmentVariable("ORDER_DB_NAME") ?? "OrderServiceDb";

        var connectionString = !string.IsNullOrEmpty(password) && password != "Your_strong_Password123"
            ? $"Host={host};Port={port};Database={dbName};Username=postgres;Password={password}"
            : configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string not available for health checks. Check 'DefaultConnection' in config or POSTGRES_PASSWORD env var.");
        }

        services.AddStandardHealthChecks(connectionString, null);

        // Refit Clients with Bulkheading (Resource Isolation)
        services.AddRefitClient<IProductServiceClient>()
            .ConfigureHttpClient((sp, c) =>
            {
                var settings = sp.GetRequiredService<IOptions<ProductServiceSettings>>().Value;
                c.BaseAddress = new Uri(settings.BaseUrl);
                c.Timeout = TimeSpan.FromSeconds(10);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                MaxConnectionsPerServer = 20, // BP #37: Bulkheading
                PooledConnectionLifetime = TimeSpan.FromMinutes(5)
            })
            .AddPolicyHandler(BuildingBlocks.Resilience.ResiliencePolicies.GetWaitAndRetryPolicy())
            .AddPolicyHandler(BuildingBlocks.Resilience.ResiliencePolicies.GetCircuitBreakerPolicy());

        services.AddRefitClient<IWeatherApiClient>()
            .ConfigureHttpClient((sp, c) =>
            {
                var settings = sp.GetRequiredService<IOptions<WeatherApiSettings>>().Value;
                c.BaseAddress = new Uri(settings.BaseUrl);
                c.Timeout = TimeSpan.FromSeconds(10);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                MaxConnectionsPerServer = 10, // BP #37: Bulkheading
                PooledConnectionLifetime = TimeSpan.FromMinutes(2)
            })
            .AddHttpMessageHandler<WeatherApiKeyHandler>()
            .AddPolicyHandler(BuildingBlocks.Resilience.ResiliencePolicies.GetWaitAndRetryPolicy())
            .AddPolicyHandler(BuildingBlocks.Resilience.ResiliencePolicies.GetCircuitBreakerPolicy());

        // MassTransit
        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<OrderServiceDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });

            x.SetKebabCaseEndpointNameFormatter();

            // Scan Infrastructure assembly for consumers
            var infrastructureAssembly = Assembly.Load("OrderService.Infrastructure");
            x.AddConsumers(infrastructureAssembly);

            var rabbitMqOptions = configuration.GetSection(RabbitMqSettings.SectionName).Get<RabbitMqSettings>() ?? new RabbitMqSettings();

            x.AddConfigureEndpointsCallback((context, name, cfg) =>
            {
                cfg.UseEntityFrameworkOutbox<OrderServiceDbContext>(context);
            });

            if (environment.IsDevelopment() && string.IsNullOrEmpty(configuration["RabbitMQ:Host"]))
            {
                x.UsingInMemory((context, cfg) =>
                {
                    // BP #16: Exponential retry for transient failures
                    cfg.UseMessageRetry(r => r.Exponential(3, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(2)));
                    cfg.ConfigureEndpoints(context);
                });
            }
            else
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqOptions.Host);
                    // BP #16: Exponential retry for transient failures
                    cfg.UseMessageRetry(r => r.Exponential(3, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(2)));
                    cfg.ConfigureEndpoints(context);
                });
            }
        });

        // OpenAPI
        services.AddOpenApiDocument(options =>
        {
            options.Title = "OrderService API";
            options.Version = "v1";
            
        });

        return services;
    }
}
