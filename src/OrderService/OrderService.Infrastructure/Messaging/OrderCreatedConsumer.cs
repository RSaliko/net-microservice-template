using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Domain.Events;

namespace OrderService.Infrastructure.Messaging;

/// <summary>
/// BP #10, #16: Messaging & MQ Reliability.
/// Demonstrates an idempotent consumer for order events.
/// </summary>
public class OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger) : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        // BP #16: Consumers must handle duplicate messages (Idempotency)
        logger.LogInformation("Consuming OrderCreatedEvent for Order {OrderId}...", context.Message.Order.Id);
        
        await Task.CompletedTask;
    }
}
