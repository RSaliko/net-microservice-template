using MassTransit;
using BuildingBlocks.Contracts.Events;
using ProductService.Application.Common.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Caching;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Saga Choreography Consumer: Handles OrderCreatedEvent from OrderService.
/// Part of the order creation saga - reserves stock for all items in the order.
/// If processing fails, publishes StockReservationFailedEvent to trigger compensation.
/// </summary>
public class OrderCreatedConsumer(IApplicationDbContext context, ILogger<OrderCreatedConsumer> logger, ICacheService cacheService) 
    : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> contextMessage)
    {
        var message = contextMessage.Message;
        logger.LogInformation("Processing OrderCreatedEvent for Order: {OrderCode}", message.OrderCode);

        try
        {
            // Inbox Pattern / Deduplication
            var inboxKey = $"inbox:order_created:{message.OrderId}";
            var processed = await cacheService.GetAsync<string>(inboxKey);
            if (processed != null)
            {
                logger.LogWarning("Duplicate event detected and skipped: {OrderId}", message.OrderId);
                return;
            }

            var productIds = message.Items.Select(x => x.ProductId).ToList();
            var products = await context.Products
                .Where(x => productIds.Contains(x.Id))
                .ToListAsync(contextMessage.CancellationToken);

            foreach (var item in message.Items)
            {
                var product = products.FirstOrDefault(x => x.Id == item.ProductId);
                if (product == null)
                {
                    throw new InvalidOperationException($"Product {item.ProductId} not found");
                }

                product.DeductStock(item.Quantity);
            }

            await context.SaveChangesAsync(contextMessage.CancellationToken);
            
            // Mark as processed - only after successful save
            await cacheService.SetAsync(inboxKey, "processed", TimeSpan.FromDays(1));
            
            await contextMessage.Publish(new StockReservedEvent(message.OrderId), contextMessage.CancellationToken);

            logger.LogInformation("Saga: Stock reserved for Order {OrderId}", message.OrderId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Saga: Error processing OrderCreatedEvent for OrderId={OrderId}. Publishing failure event.", 
                message.OrderId);
            
            // Saga Choreography: Publish failure event to trigger compensation in OrderService
            await contextMessage.Publish(new StockReservationFailedEvent(
                message.OrderId,
                $"Stock reservation failed: {ex.Message}"), contextMessage.CancellationToken);
            
            // Do not rethrow if we want the saga to continue with failure path and not retry indefinitely if business logic failed
            // However, for transient errors (DB down), rethrowing is good.
            // For logic errors (Insufficient stock), we've published the failure event, so we can complete the message.
            if (ex is InvalidOperationException) return;
            
            throw;
        }
    }
}
