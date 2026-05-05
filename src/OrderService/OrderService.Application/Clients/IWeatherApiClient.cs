using Refit;
using System.Text.Json.Serialization;

namespace OrderService.Application.Clients;

public interface IWeatherApiClient
{
    [Get("/v1/current.json")]
    Task<WeatherCurrentResponse> GetCurrentAsync(
        [AliasAs("q")] string query,
        [AliasAs("aqi")] string aqi = "no",
        CancellationToken cancellationToken = default);
}

public record WeatherCurrentResponse(
    [property: JsonPropertyName("location")]
    WeatherLocationDto Location,
    [property: JsonPropertyName("current")]
    WeatherCurrentDto Current
);

public record WeatherLocationDto(
    [property: JsonPropertyName("name")]
    string Name,
    [property: JsonPropertyName("region")]
    string Region,
    [property: JsonPropertyName("country")]
    string Country,
    [property: JsonPropertyName("lat")]
    double Lat,
    [property: JsonPropertyName("lon")]
    double Lon,
    [property: JsonPropertyName("tz_id")]
    string TzId,
    [property: JsonPropertyName("localtime_epoch")]
    long LocaltimeEpoch,
    [property: JsonPropertyName("localtime")]
    string Localtime
);

public record WeatherCurrentDto(
    [property: JsonPropertyName("last_updated_epoch")]
    long LastUpdatedEpoch,
    [property: JsonPropertyName("last_updated")]
    string LastUpdated,
    [property: JsonPropertyName("temp_c")]
    double TempC,
    [property: JsonPropertyName("temp_f")]
    double TempF,
    [property: JsonPropertyName("is_day")]
    int IsDay,
    [property: JsonPropertyName("condition")]
    WeatherConditionDto Condition,
    [property: JsonPropertyName("wind_kph")]
    double WindKph,
    [property: JsonPropertyName("humidity")]
    int Humidity,
    [property: JsonPropertyName("cloud")]
    int Cloud,
    [property: JsonPropertyName("feelslike_c")]
    double FeelslikeC,
    [property: JsonPropertyName("uv")]
    double Uv
);

public record WeatherConditionDto(
    [property: JsonPropertyName("text")]
    string Text,
    [property: JsonPropertyName("icon")]
    string Icon,
    [property: JsonPropertyName("code")]
    int Code
);
