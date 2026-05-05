using OrderService.Application.Clients;

namespace OrderService.Application.Common.Interfaces;

public interface IWeatherQueryService
{
    Task<WeatherCurrentResponse> GetCurrentAsync(string query, string aqi, CancellationToken cancellationToken);
}
