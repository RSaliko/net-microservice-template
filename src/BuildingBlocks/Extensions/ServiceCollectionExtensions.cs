using BuildingBlocks.ExceptionHandlers;
using BuildingBlocks.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.RateLimiting;

namespace BuildingBlocks.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddBuildingBlocks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        
        // Caching
        var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost";
        var redisPort = Environment.GetEnvironmentVariable("REDIS_PORT") ?? "6379";
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = $"{redisHost}:{redisPort}";
            options.InstanceName = "App_";
        });
        
        services.AddScoped<BuildingBlocks.Caching.ICacheService, BuildingBlocks.Caching.CacheService>();
        
        // API Behavior
        services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .Select(x => new
                    {
                        PropertyName = x.Key,
                        ErrorMessages = x.Value!.Errors.Select(e => e.ErrorMessage).ToList()
                    }).ToList();

                var response = new ApiResponse(400, "Validation failed.", errors);
                return new BadRequestObjectResult(response);
            };
        });

        // Rate Limiting
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("fixed", opt =>
            {
                opt.Window = TimeSpan.FromSeconds(10);
                opt.PermitLimit = 100;
            });
        });

        return services;
    }

    public static IApplicationBuilder UseBuildingBlocks(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();
        return app;
    }
}
