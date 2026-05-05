using MassTransit;
using Microsoft.Extensions.Logging;

namespace ProductService.Infrastructure.Messaging.Consumers;

public record ProductCreatedEvent(Guid Id, string Title);

public class ProductCreatedConsumer : IConsumer<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedConsumer> _logger;

    public ProductCreatedConsumer(ILogger<ProductCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductCreatedEvent> context)
    {
        // Rule: Consumer must handle duplicate messages (Inbox Pattern)
        // Check if message has been processed before using context.MessageId
        
        _logger.LogInformation("Product Created Event consumed: {ProductId} - {Title}", context.Message.Id, context.Message.Title);
        await Task.CompletedTask;
    }
}
