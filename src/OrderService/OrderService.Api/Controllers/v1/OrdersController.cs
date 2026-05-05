using BuildingBlocks.Controllers;
using BuildingBlocks.Filters;
using BuildingBlocks.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Features.Orders.DTOs;
using OrderService.Application.Features.Orders.Commands.CreateOrder;
using OrderService.Application.Features.Orders.Queries.GetOrderSummary;

using Microsoft.AspNetCore.Authorization;

namespace OrderService.Api.Controllers.v1;

/// <summary>
/// Controller for managing orders and demonstrating cross-service integration.
/// </summary>
// [Authorize]
public class OrdersController : BaseApiController
{
    /// <summary>
    /// Gets a summary of orders with product details.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Order summary with product details.</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<OrderSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetOrderSummaryQuery(), cancellationToken);
        return Ok(ApiResponse<OrderSummaryDto>.SuccessResponse(result, "Aggregated order summary with real-time product data fetched via Refit."));
    }

    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <param name="command">Order placement details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Order ID of the created order.</returns>
    [HttpPost]
    [Idempotent]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(
            nameof(GetSummary),
            null,
            ApiResponse<Guid>.CreatedResponse(id, "Order created successfully. Outbox message scheduled for ProductService synchronization."));
    }
}
