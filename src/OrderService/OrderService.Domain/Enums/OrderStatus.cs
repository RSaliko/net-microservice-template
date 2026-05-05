namespace OrderService.Domain.Enums;

public enum OrderStatus
{
    Draft = 1,
    Submitted = 2,
    StockReserved = 3,
    Cancelled = 4,
    Active = 5,
    Inactive = 6
}
