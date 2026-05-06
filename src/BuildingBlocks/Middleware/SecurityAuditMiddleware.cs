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
        var traceId = System.Diagnostics.Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
        var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value);
        
        // Log sensitive operations (POST, PUT, DELETE)
        if (method != HttpMethods.Get && method != HttpMethods.Options)
        {
            _logger.LogInformation("Security Audit: User {User} (Roles: {Roles}) performing {Method} on {Path} [TraceId: {TraceId}]", 
                user, string.Join(",", roles), method, path, traceId);
            
            // In a real app, we'd log a sanitized body here
        }

        await _next(context);

        if (context.Response.StatusCode == StatusCodes.Status401Unauthorized || context.Response.StatusCode == StatusCodes.Status403Forbidden)
        {
            _logger.LogWarning("Security Audit: User {User} denied access to {Path} (Status: {StatusCode}) [TraceId: {TraceId}]", 
                user, path, context.Response.StatusCode, traceId);
        }
    }
}
