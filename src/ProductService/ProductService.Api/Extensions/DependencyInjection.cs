using System.Reflection;
using Asp.Versioning;
using BuildingBlocks.HealthChecks;
using BuildingBlocks.Settings;
using MassTransit;
using Microsoft.Extensions.Options;
using ProductService.Persistence.Contexts;

namespace ProductService.Api.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
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
        var dbName = Environment.GetEnvironmentVariable("PRODUCT_DB_NAME") ?? "ProductServiceDb";

        var connectionString = !string.IsNullOrEmpty(password) && password != "Your_strong_Password123"
            ? $"Host={host};Port={port};Database={dbName};Username=postgres;Password={password}"
            : configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string not available for health checks. Check 'DefaultConnection' in config or POSTGRES_PASSWORD env var.");
        }

        services.AddStandardHealthChecks(connectionString, null);

        // MassTransit
        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<ProductServiceDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });

            x.SetKebabCaseEndpointNameFormatter();

            // Consumers
            // Scan Infrastructure assembly for consumers
            var infrastructureAssembly = Assembly.Load("ProductService.Infrastructure");
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
            options.Title = "ProductService API";
            options.Version = "v1";
            
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
