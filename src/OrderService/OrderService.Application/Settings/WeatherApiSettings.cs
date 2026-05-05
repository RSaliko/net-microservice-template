using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.Settings;

public class WeatherApiSettings
{
    public const string SectionName = "WeatherApi";

    [Required]
    [Url]
    public string BaseUrl { get; init; } = "https://api.weatherapi.com";

    [Required]
    [MinLength(16)]
    public string ApiKey { get; init; } = string.Empty;
}
