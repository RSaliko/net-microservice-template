using BuildingBlocks.Exceptions;
using BuildingBlocks.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.ExceptionHandlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

        var (statusCode, message, errorCode) = exception switch
        {
            BusinessException busEx => (busEx.StatusCode, busEx.Message, busEx.ErrorCode),
            _ => (500, "An internal server error occurred.", BuildingBlocks.Common.ErrorCodes.System.InternalError)
        };

        var response = new ApiResponse(statusCode, message, null, errorCode);
        
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken).ConfigureAwait(false);

        return true;
    }
}

public class ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<ValidationExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not FluentValidation.ValidationException validationException) return false;

        _logger.LogWarning(validationException, "Validation failed: {Errors}", 
            string.Join(", ", validationException.Errors.Select(e => $"{e.PropertyName}:{e.ErrorMessage}")));

        try
        {
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            var response = new ApiResponse(400, "Validation Failed", errors, BuildingBlocks.Common.ErrorCodes.Validation.GeneralFailure);
            
            httpContext.Response.StatusCode = 400;
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken).ConfigureAwait(false);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to serialize validation error response");
            return false;
        }
    }
}
