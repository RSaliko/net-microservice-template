using MassTransit;
using BuildingBlocks.Contracts.Events;
using ProductService.Application.Common.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Caching;

namespace ProductService.Infrastructure.Messaging.Consumers;

public class OrderCreatedConsumer(IApplicationDbContext context, ILogger<OrderCreatedConsumer> logger, ICacheService cacheService) 
    : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> contextMessage)
    {
        var message = contextMessage.Message;
        logger.LogInformation("Processing OrderCreatedEvent for Order: {OrderCode}", message.OrderCode);

        // Inbox Pattern / Deduplication
        var inboxKey = $"inbox:order_created:{message.OrderId}";
        var processed = await cacheService.GetAsync<string>(inboxKey);
        if (processed != null)
        {
            logger.LogWarning("Duplicate event detected and skipped: {OrderId}", message.OrderId);
            return;
        }

        var sku = $"PRJ-{message.OrderId:N}"[..32];
        var product = new Product(
            sku,
            $"Setup for {message.OrderCode}", 
            $"Automatic setup product for Order {message.OrderId}",
            99.99m,
            100);

        context.Products.Add(product);
        product.Activate();
        await context.SaveChangesAsync(contextMessage.CancellationToken);
        
        // Mark as processed
        await cacheService.SetAsync(inboxKey, "processed", TimeSpan.FromDays(1));
        
        logger.LogInformation("Created and activated automatic product {ProductId} for Order {OrderId}", product.Id, message.OrderId);
    }
}
