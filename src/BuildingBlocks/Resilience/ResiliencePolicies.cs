using Polly;
using Polly.Extensions.Http;
using System.Net.Http;

namespace BuildingBlocks.Resilience;

public static class ResiliencePolicies
{
    /// <summary>
    /// BP #29: Centralized Retry Policy with Jitter.
    /// Handles Transient HTTP Errors (5xx, 408) and Network failures.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetWaitAndRetryPolicy(int retryCount = 3)
    {
        return HttpPolicyExtensions.HandleTransientHttpError()
            .WaitAndRetryAsync(retryCount, retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) 
                + TimeSpan.FromMilliseconds(new Random().Next(0, 100)));
    }

    /// <summary>
    /// BP #29: Circuit Breaker Policy.
    /// Breaks the circuit after specified number of consecutive failures.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int handledEventsAllowedBeforeBreaking = 5, int durationOfBreakInSeconds = 30)
    {
        return HttpPolicyExtensions.HandleTransientHttpError()
            .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking, TimeSpan.FromSeconds(durationOfBreakInSeconds));
    }
}
