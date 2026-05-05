using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace BuildingBlocks.Middleware;

public class SecurityAuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityAuditMiddleware> _logger;

    public SecurityAuditMiddleware(RequestDelegate next, ILogger<SecurityAuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.User.Identity?.Name ?? "Anonymous";
        var path = context.Request.Path;
        var method = context.Request.Method;

        // Log sensitive operations (POST, PUT, DELETE)
        if (method != HttpMethods.Get && method != HttpMethods.Options)
        {
            _logger.LogInformation("Security Audit: User {User} performing {Method} on {Path}", user, method, path);
        }

        await _next(context);

        if (context.Response.StatusCode == StatusCodes.Status401Unauthorized || context.Response.StatusCode == StatusCodes.Status403Forbidden)
        {
            _logger.LogWarning("Security Audit: User {User} denied access to {Path} (Status: {StatusCode})", user, path, context.Response.StatusCode);
        }
    }
}
