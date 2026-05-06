using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace OrderService.Infrastructure.Services;

/// <summary>
/// BP #29: External Service Resilience (Polly Deep-dive).
/// Demonstrates a service with retry and circuit breaker logic.
/// </summary>
public class PaymentService
{
    private static readonly Random _random = new();
    private readonly ILogger<PaymentService> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public PaymentService(ILogger<PaymentService> logger)
    {
        _logger = logger;
        
        // BP #29: Retry with Jitter
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(_random.Next(0, 100)));
    }

    public async Task<bool> ProcessPaymentAsync(Guid orderId, decimal amount, CancellationToken cancellationToken)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            _logger.LogInformation("Processing payment for Order {OrderId}...", orderId);
            
            // Simulate external call
            await Task.Delay(100, cancellationToken);
            
            return true;
        });
    }
}
