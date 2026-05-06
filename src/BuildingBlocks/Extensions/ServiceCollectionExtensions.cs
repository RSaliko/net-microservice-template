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
        
        // Caching & Redis
        var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost";
        var redisPort = Environment.GetEnvironmentVariable("REDIS_PORT") ?? "6379";
        var redisConnectionString = $"{redisHost}:{redisPort}";

        services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp => 
            StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnectionString));

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "App_";
        });
        
        services.AddScoped<BuildingBlocks.Caching.ICacheService, BuildingBlocks.Caching.CacheService>();
        services.AddSingleton<BuildingBlocks.Caching.IDistributedLockService, BuildingBlocks.Caching.RedisDistributedLockService>();
        
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

        // CORS
        services.AddCors(options =>
        {
            options.AddPolicy("DefaultCorsPolicy", builder =>
            {
                builder.WithOrigins("http://localhost:4200", "http://localhost:4000")
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials();
            });
        });

        return services;
    }

    public static IApplicationBuilder UseBuildingBlocks(this IApplicationBuilder app)
    {
        app.UseCors("DefaultCorsPolicy");
        app.UseExceptionHandler();
        app.UseMiddleware<BuildingBlocks.Middleware.SecurityHeadersMiddleware>();
        app.UseMiddleware<BuildingBlocks.Middleware.ETagMiddleware>();
        app.UseMiddleware<BuildingBlocks.Middleware.SecurityAuditMiddleware>();
        return app;
    }
}
