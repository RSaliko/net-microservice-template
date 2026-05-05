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
        var host = Environment.GetEnvironmentVariable("MSSQL_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("MSSQL_PORT") ?? "14333";
        var password = Environment.GetEnvironmentVariable("MSSQL_SA_PASSWORD") ?? "Your_strong_Password123";
        var dbName = Environment.GetEnvironmentVariable("ORDER_DB_NAME") ?? "OrderServiceDb";

        // Match Persistence DependencyInjection logic: use config as fallback if default password
        var connectionString = !string.IsNullOrEmpty(password) && password != "Your_strong_Password123"
            ? $"Server={host},{port};Database={dbName};User Id=sa;Password={password};TrustServerCertificate=True;Encrypt=False"
            : configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string not available for health checks. Check 'DefaultConnection' in config or MSSQL_SA_PASSWORD env var.");
        }

        services.AddStandardHealthChecks(connectionString, null);

        // Refit Clients
        services.AddRefitClient<IProductServiceClient>()
            .ConfigureHttpClient((sp, c) =>
            {
                var settings = sp.GetRequiredService<IOptions<ProductServiceSettings>>().Value;
                c.BaseAddress = new Uri(settings.BaseUrl);
                c.Timeout = TimeSpan.FromSeconds(10);
            });

        services.AddRefitClient<IWeatherApiClient>()
            .ConfigureHttpClient((sp, c) =>
            {
                var settings = sp.GetRequiredService<IOptions<WeatherApiSettings>>().Value;
                c.BaseAddress = new Uri(settings.BaseUrl);
                c.Timeout = TimeSpan.FromSeconds(10);
            })
            .AddHttpMessageHandler<WeatherApiKeyHandler>()
            .AddPolicyHandler(BuildingBlocks.Resilience.ResiliencePolicies.GetWaitAndRetryPolicy());

        // MassTransit
        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<OrderServiceDbContext>(o =>
            {
                o.UseSqlServer();
                o.UseBusOutbox();
            });

            x.SetKebabCaseEndpointNameFormatter();

            // Scan Infrastructure assembly for consumers
            var infrastructureAssembly = Assembly.Load("OrderService.Infrastructure");
            x.AddConsumers(infrastructureAssembly);

            var rabbitMqOptions = configuration.GetSection(RabbitMqSettings.SectionName).Get<RabbitMqSettings>() ?? new RabbitMqSettings();

            if (environment.IsDevelopment() && string.IsNullOrEmpty(configuration["RabbitMQ:Host"]))
            {
                x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
            }
            else
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqOptions.Host);
                    cfg.ConfigureEndpoints(context);
                });
            }
        });

        // OpenAPI
        services.AddOpenApiDocument(options =>
        {
            options.Title = "OrderService API";
            options.Version = "v1";
            
            // BP #5: Load XML documentation for better API docs
            var assemblies = new[]
            {
                Assembly.GetExecutingAssembly(),
                Assembly.Load("OrderService.Application"),
                Assembly.Load("OrderService.Domain")
            };

            /*
            foreach (var assembly in assemblies)
            {
                options.AddXmlDocumentationParameters(assembly);
            }
            */
            
            options.AddSecurity("JWT", Enumerable.Empty<string>(), new NSwag.OpenApiSecurityScheme
            {
                Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
                Name = "Authorization",
                In = NSwag.OpenApiSecurityApiKeyLocation.Header,
                Description = "Type into the textbox: Bearer {your JWT token}."
            });

            options.OperationProcessors.Add(
                new NSwag.Generation.Processors.Security.AspNetCoreOperationSecurityScopeProcessor("JWT"));
        });

        return services;
    }
}
