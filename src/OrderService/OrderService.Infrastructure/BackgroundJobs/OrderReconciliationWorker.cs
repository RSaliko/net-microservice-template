using BuildingBlocks.Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderService.Application.Common.Interfaces;
using OrderService.Domain.Enums;

namespace OrderService.Infrastructure.BackgroundJobs;

/// <summary>
/// BP #36: Reconciliation background worker to fix eventual consistency gaps.
/// </summary>
public class OrderReconciliationWorker(
    ILogger<OrderReconciliationWorker> logger, 
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Order Reconciliation Worker is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ReconcileOrdersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during order reconciliation.");
            }

            // Run every hour
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task ReconcileOrdersAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        
        logger.LogInformation("Reconciling orders at {Time}", DateTimeOffset.Now);

        // BP #36: Find orders stuck in 'Submitted' state for > 30 minutes
        // These might have missed their 'StockReserved' event due to messaging failures.
        var staleOrders = await context.Orders
            .Include(o => o.Items)
            .AsSplitQuery() // BP #24: Optimized for collection includes
            .Where(o => o.Status == OrderStatus.Submitted && o.CreatedAt < DateTimeOffset.UtcNow.AddMinutes(-30))
            .ToListAsync(cancellationToken);

        if (staleOrders.Count == 0)
        {
            logger.LogInformation("No stale orders found.");
            return;
        }

        logger.LogInformation("Found {Count} stale orders. Re-publishing events...", staleOrders.Count);

        foreach (var order in staleOrders)
        {
            await publishEndpoint.Publish(new OrderCreatedEvent(
                order.Id,
                order.OrderCode,
                order.ReceiverName,
                order.CreatedAt,
                order.Items.Select(x => new OrderItemEvent(x.ProductId, x.Quantity)).ToList()), cancellationToken);
            
            logger.LogInformation("Re-published OrderCreatedEvent for Order {OrderId}", order.Id);
        }
    }
}
