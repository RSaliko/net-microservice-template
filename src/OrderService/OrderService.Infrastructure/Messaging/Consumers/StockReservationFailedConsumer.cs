using BuildingBlocks.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Infrastructure.Messaging.Consumers;

public class StockReservationFailedConsumer(
    IApplicationDbContext context, 
    ILogger<StockReservationFailedConsumer> logger) : IConsumer<StockReservationFailedEvent>
{
    public async Task Consume(ConsumeContext<StockReservationFailedEvent> contextMessage)
    {
        var message = contextMessage.Message;
        logger.LogWarning("Saga: Stock reservation failed for Order {OrderId}. Reason: {Reason}. Cancelling order.", 
            message.OrderId, message.Reason);

        var order = await context.Orders
            .FirstOrDefaultAsync(x => x.Id == message.OrderId, contextMessage.CancellationToken);

        if (order == null)
        {
            logger.LogError("Order {OrderId} not found during StockReservationFailed processing", message.OrderId);
            return;
        }

        order.Cancel();
        await context.SaveChangesAsync(contextMessage.CancellationToken);
        
        logger.LogInformation("Saga: Order {OrderId} cancelled due to stock reservation failure", message.OrderId);
    }
}
