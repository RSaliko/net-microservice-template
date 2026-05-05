namespace OrderService.Domain.Exceptions;

public class OrderNotFoundException : Exception
{
    public OrderNotFoundException(Guid id) : base($"Order with ID {id} was not found.") { }
}
