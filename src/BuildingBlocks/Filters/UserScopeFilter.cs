using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Filters;

public class UserScopeFilter : IAsyncActionFilter
{
    private readonly ILogger<UserScopeFilter> _logger;

    public UserScopeFilter(ILogger<UserScopeFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userId = context.HttpContext.User.Identity?.Name ?? "anonymous";
        
        using (_logger.BeginScope(new Dictionary<string, object> { ["UserId"] = userId }))
        {
            await next();
        }
    }
}
