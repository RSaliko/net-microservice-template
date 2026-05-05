using MassTransit;
using Microsoft.Extensions.Logging;

namespace ProductService.Infrastructure.Messaging;

public record ItemCreatedIntegrationEvent(Guid Id, string Name);

public class ItemCreatedConsumer : IConsumer<ItemCreatedIntegrationEvent>
{
    private readonly ILogger<ItemCreatedConsumer> _logger;

    public ItemCreatedConsumer(ILogger<ItemCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ItemCreatedIntegrationEvent> context)
    {
        _logger.LogInformation("Consumed ItemCreatedIntegrationEvent: {Id} - {Name}", context.Message.Id, context.Message.Name);
        return Task.CompletedTask;
    }
}
