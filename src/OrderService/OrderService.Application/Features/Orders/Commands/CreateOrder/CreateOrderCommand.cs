using BuildingBlocks.Contracts.Events;
using MassTransit;
using BuildingBlocks.Data;
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

public class CreateOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork, IPublishEndpoint publishEndpoint) 
    : IRequestHandler<CreateOrderCommand, Guid>
{
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
        
        order.Submit();

        await orderRepository.AddAsync(order, cancellationToken);
        
        // BP: Event-driven synchronization via Outbox (must be called BEFORE SaveChangesAsync)
        await publishEndpoint.Publish(new OrderCreatedEvent(
            order.Id,
            order.OrderCode,
            order.ReceiverName,
            order.CreatedAt,
            order.Items.Select(x => new OrderItemEvent(x.ProductId, x.Quantity)).ToList()), cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}
