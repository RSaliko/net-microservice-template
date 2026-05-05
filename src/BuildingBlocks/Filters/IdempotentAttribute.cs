using BuildingBlocks.Caching;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Filters;

[AttributeUsage(AttributeTargets.Method)]
public class IdempotentAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue("X-Idempotency-Key", out var idempotencyKey))
        {
            await next();
            return;
        }

        var cacheService = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();
        var cacheKey = $"idempotency:{idempotencyKey}";

        var cachedResult = await cacheService.GetAsync<object>(cacheKey);
        if (cachedResult != null)
        {
            context.Result = new ConflictObjectResult(new { message = "Duplicate request detected with the same idempotency key." });
            return;
        }

        // Mark as processing
        await cacheService.SetAsync(cacheKey, "processing", TimeSpan.FromMinutes(10));

        var executedContext = await next();

        // Only cache as processed if request succeeded (2xx)
        if (executedContext.HttpContext.Response.StatusCode >= 200 && executedContext.HttpContext.Response.StatusCode < 300)
        {
            // In a real scenario, we would cache the actual response here.
            // For now, we just leave it as "processed" to prevent immediate retries.
            await cacheService.SetAsync(cacheKey, "processed", TimeSpan.FromHours(1));
        }
        else
        {
            // Remove idempotency key on failure to allow retries
            await cacheService.RemoveAsync(cacheKey);
        }
    }
}
