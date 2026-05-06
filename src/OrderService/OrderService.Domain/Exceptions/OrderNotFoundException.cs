using BuildingBlocks.Exceptions;

namespace OrderService.Domain.Exceptions;

public class OrderNotFoundException : NotFoundException
{
    public OrderNotFoundException(Guid id) 
        : base($"Order with ID {id} was not found.", "ORD_NOT_FOUND") { }
}
