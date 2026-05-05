using System.Net.Http;

namespace ProductService.Infrastructure.Clients;

/// <summary>
/// Example of a resilient external service client.
/// Follows Rule 191 (Polly) and Rule 243 (Service Timeouts).
/// </summary>
public class ExternalServiceClient
{
    private readonly HttpClient _httpClient;

    public ExternalServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(10); // Rule 244: Mandatory Timeout
    }
}
