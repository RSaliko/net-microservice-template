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

        var cachedResult = await cacheService.GetAsync<IdempotentResponse>(cacheKey);
        if (cachedResult != null)
        {
            if (cachedResult.Status == "processing")
            {
                context.Result = new ConflictObjectResult(new { message = "Request is currently being processed." });
                return;
            }

            context.Result = new ObjectResult(cachedResult.Data) { StatusCode = cachedResult.StatusCode };
            return;
        }

        // Mark as processing
        await cacheService.SetAsync(cacheKey, new IdempotentResponse { Status = "processing" }, TimeSpan.FromMinutes(5));

        var executedContext = await next();

        // Only cache as processed if request succeeded (2xx)
        if (executedContext.HttpContext.Response.StatusCode >= 200 && executedContext.HttpContext.Response.StatusCode < 300)
        {
            object? responseData = null;
            if (executedContext.Result is ObjectResult objectResult)
            {
                responseData = objectResult.Value;
            }

            await cacheService.SetAsync(cacheKey, new IdempotentResponse 
            { 
                Status = "processed", 
                StatusCode = executedContext.HttpContext.Response.StatusCode,
                Data = responseData
            }, TimeSpan.FromHours(1));
        }
        else
        {
            // Remove idempotency key on failure to allow retries
            await cacheService.RemoveAsync(cacheKey);
        }
    }
}

public class IdempotentResponse
{
    public string Status { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public object? Data { get; set; }
}
