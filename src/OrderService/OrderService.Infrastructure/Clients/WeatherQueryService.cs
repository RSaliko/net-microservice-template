using OrderService.Application.Clients;
using OrderService.Application.Common.Interfaces;

namespace OrderService.Infrastructure.Clients;

public sealed class WeatherQueryService : IWeatherQueryService
{
    private readonly IWeatherApiClient _weatherApiClient;

    public WeatherQueryService(IWeatherApiClient weatherApiClient)
    {
        _weatherApiClient = weatherApiClient;
    }

    public Task<WeatherCurrentResponse> GetCurrentAsync(string query, string aqi, CancellationToken cancellationToken)
    {
        var safeAqi = string.IsNullOrWhiteSpace(aqi) ? "no" : aqi;
        return _weatherApiClient.GetCurrentAsync(query, safeAqi, cancellationToken);
    }
}
