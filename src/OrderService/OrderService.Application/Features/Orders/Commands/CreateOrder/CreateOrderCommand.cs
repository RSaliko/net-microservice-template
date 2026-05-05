using BuildingBlocks.Contracts.Events;
using MassTransit;
using MediatR;
using OrderService.Application.Common.Interfaces;
using OrderService.Domain.Entities;

namespace OrderService.Application.Features.Orders.Commands.CreateOrder;

public record CreateOrderItemCommand(Guid ProductId, int Quantity, decimal UnitPrice);

public record CreateOrderCommand(
    string OrderCode, 
    string ReceiverName, 
    string Phone, 
    string Email, 
    string Note, 
    IReadOnlyCollection<CreateOrderItemCommand> Items) : IRequest<Guid>;

public class CreateOrderCommandHandler(IApplicationDbContext context, IPublishEndpoint publishEndpoint) 
    : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // BP: Domain logic encapsulated in the Entity
        var order = new Order(
            request.OrderCode, 
            request.ReceiverName, 
            request.Phone, 
            request.Email, 
            request.Note);

        foreach (var item in request.Items)
        {
            order.AddItem(item.ProductId, item.Quantity, item.UnitPrice);
        }
        
        _context.Orders.Add(order);
        
        // BP: Event-driven synchronization via Outbox (must be called BEFORE SaveChangesAsync)
        await _publishEndpoint.Publish(new OrderCreatedEvent(
            order.Id,
            order.OrderCode,
            order.ReceiverName,
            order.CreatedAt), cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}
