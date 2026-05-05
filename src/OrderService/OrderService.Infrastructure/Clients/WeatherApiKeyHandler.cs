using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using OrderService.Application.Settings;
using System.Net.Http;

namespace OrderService.Infrastructure.Clients;

public sealed class WeatherApiKeyHandler : DelegatingHandler
{
    private readonly IOptions<WeatherApiSettings> _settings;

    public WeatherApiKeyHandler(IOptions<WeatherApiSettings> settings)
    {
        _settings = settings;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var requestUri = request.RequestUri?.ToString();
        if (string.IsNullOrWhiteSpace(requestUri))
        {
            return base.SendAsync(request, cancellationToken);
        }

        var existing = QueryHelpers.ParseQuery(new Uri(requestUri).Query);
        if (!existing.ContainsKey("key"))
        {
            request.RequestUri = new Uri(QueryHelpers.AddQueryString(requestUri, "key", _settings.Value.ApiKey));
        }

        return base.SendAsync(request, cancellationToken);
    }
}
