using BuildingBlocks.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Infrastructure.Messaging.Consumers;

public class StockReservedConsumer(
    IApplicationDbContext context, 
    ILogger<StockReservedConsumer> logger) : IConsumer<StockReservedEvent>
{
    public async Task Consume(ConsumeContext<StockReservedEvent> contextMessage)
    {
        var message = contextMessage.Message;
        logger.LogInformation("Saga: Stock reserved for Order {OrderId}. Updating status to StockReserved.", message.OrderId);

        var order = await context.Orders
            .FirstOrDefaultAsync(x => x.Id == message.OrderId, contextMessage.CancellationToken);

        if (order == null)
        {
            logger.LogError("Order {OrderId} not found during StockReserved processing", message.OrderId);
            return;
        }

        order.ReserveStock();
        await context.SaveChangesAsync(contextMessage.CancellationToken);
        
        logger.LogInformation("Saga: Order {OrderId} status updated to StockReserved", message.OrderId);
    }
}
