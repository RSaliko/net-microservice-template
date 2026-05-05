using BuildingBlocks.Controllers;
using BuildingBlocks.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Features.Products.Commands.CreateProduct;
using ProductService.Application.Features.Products.Queries.GetProduct;
using ProductService.Application.Features.Products.Queries.GetProducts;
using ProductService.Application.Features.Products.DTOs;

using Microsoft.AspNetCore.Authorization;

namespace ProductService.Api.Controllers.v1;

/// <summary>
/// Controller for managing products.
/// </summary>
// [Authorize]
public class ProductsController : BaseApiController
{
    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="command">The details of the product to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Product ID of the created product.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(
            nameof(Get),
            new { id },
            ApiResponse<Guid>.CreatedResponse(id, "Product created successfully."));
    }

    /// <summary>
    /// Gets a product by ID.
    /// </summary>
    /// <param name="id">The GUID of the product.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Product details or 404 if not found.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetProductQuery(id), cancellationToken);
        return Ok(ApiResponse<ProductDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Gets a paginated list of products.
    /// </summary>
    /// <param name="query">Paging, sorting, and filtering parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated list of products.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<ProductDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProducts([FromQuery] GetProductsQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<PaginatedResult<ProductDto>>.SuccessResponse(result));
    }
}
