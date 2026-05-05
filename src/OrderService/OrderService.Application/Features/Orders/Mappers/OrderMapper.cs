using OrderService.Application.Features.Orders.DTOs;
using OrderService.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace OrderService.Application.Features.Orders.Mappers;

/// <summary>
/// BP #5: Mapperly for source-generated mapping.
/// </summary>
[Mapper]
public partial class OrderMapper
{
    public partial OrderDto OrderToOrderDto(Order order);
    
    // Custom mapping logic if needed
    private static decimal MapTotalAmount(Order order) => order.TotalAmount;
}
