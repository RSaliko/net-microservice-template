using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Common.Interfaces;
using OrderService.Infrastructure.Clients;

namespace OrderService.Infrastructure;

/// <summary>
/// Infrastructure layer DI registration.
/// External HTTP clients (Refit) are registered in the API layer (Program.cs)
/// using type-safe IOptions so they can access IConfiguration directly.
/// Add database repositories, external adapters, or background services here.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // External adapters and infrastructure services belong here.
        services.AddScoped<IWeatherQueryService, WeatherQueryService>();
        services.AddTransient<WeatherApiKeyHandler>();

        // Example: services.AddScoped<IPaymentGateway, StripePaymentGateway>();
        return services;
    }
}
