using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Controllers;

/// <summary>
/// Base API Controller providing common properties and MediatR integration.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("fixed")]
public abstract class BaseApiController : ControllerBase
{
    private IMediator? _mediator;

    /// <summary>
    /// Lazy-loaded Mediator instance.
    /// </summary>
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();
}
