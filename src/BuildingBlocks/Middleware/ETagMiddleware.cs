using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace BuildingBlocks.Middleware;

public class ETagMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method != HttpMethods.Get)
        {
            await next(context);
            return;
        }

        var originalStream = context.Response.Body;
        using var ms = new MemoryStream();
        context.Response.Body = ms;

        await next(context);

        if (context.Response.StatusCode != StatusCodes.Status200OK || ms.Length == 0)
        {
            ms.Position = 0;
            await ms.CopyToAsync(originalStream);
            context.Response.Body = originalStream;
            return;
        }

        var etag = GenerateETag(ms);

        if (context.Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var incomingEtag) && incomingEtag == etag)
        {
            context.Response.StatusCode = StatusCodes.Status304NotModified;
            context.Response.ContentLength = 0;
            context.Response.Body = originalStream;
            return;
        }

        context.Response.Headers[HeaderNames.ETag] = etag;
        ms.Position = 0;
        await ms.CopyToAsync(originalStream);
        context.Response.Body = originalStream;
    }

    private static string GenerateETag(MemoryStream ms)
    {
        ms.Position = 0;
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(ms);
        return $"\"{Convert.ToBase64String(hash)}\"";
    }
}
