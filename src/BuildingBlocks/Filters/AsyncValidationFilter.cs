using BuildingBlocks.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BuildingBlocks.Filters;

public class AsyncValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .Select(x => new
                {
                    PropertyName = x.Key,
                    ErrorMessages = x.Value!.Errors.Select(e => e.ErrorMessage).ToList()
                })
                .ToList();

            var response = new ApiResponse(400, "Validation failed.", errors);
            context.Result = new BadRequestObjectResult(response);
            return;
        }

        await next();
    }
}
