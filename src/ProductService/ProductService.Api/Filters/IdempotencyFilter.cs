using BuildingBlocks.Common;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProductService.Api.Filters;

/// <summary>
/// Placeholder for X-Idempotency-Key check.
/// Ensures write operations are idempotent across retries.
/// </summary>
public class IdempotencyFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.ContainsKey(Constants.Headers.IdempotencyKey))
        {
            // Implementation logic here
        }

        await next();
    }
}
