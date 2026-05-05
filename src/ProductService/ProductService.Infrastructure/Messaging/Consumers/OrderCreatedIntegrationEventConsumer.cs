using BuildingBlocks.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ProductService.Infrastructure.Messaging.Consumers;

public class OrderCreatedIntegrationEventConsumer : IConsumer<OrderCreatedIntegrationEvent>
{
    private readonly ILogger<OrderCreatedIntegrationEventConsumer> _logger;

    public OrderCreatedIntegrationEventConsumer(ILogger<OrderCreatedIntegrationEventConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<OrderCreatedIntegrationEvent> context)
    {
        _logger.LogInformation("ProductService consumed OrderCreatedIntegrationEvent: OrderId={OrderId}, Customer={CustomerName}, Total={TotalAmount}", 
            context.Message.OrderId, context.Message.CustomerName, context.Message.TotalAmount);
            
        // Logic to reserve stock or handle order creation
        return Task.CompletedTask;
    }
}
