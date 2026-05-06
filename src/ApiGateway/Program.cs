using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Serilog;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog Structured Logging ────────────────────────────────────────────────
builder.Logging.ClearProviders();
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "ApiGateway"));

// ── Graceful Shutdown ─────────────────────────────────────────────────────────
builder.Services.Configure<HostOptions>(options =>
{
    options.ShutdownTimeout = TimeSpan.FromSeconds(30);
});

// ── Data Protection (Persistent Keys) ─────────────────────────────────────────
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "keys")));

// ── YARP Reverse Proxy ────────────────────────────────────────────────────────
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddRequestTimeouts();

// ── Performance & Compression ─────────────────────────────────────────────────
builder.Services.AddResponseCompression(options => { options.EnableForHttps = true; });

// ── Health Checks ─────────────────────────────────────────────────────────────
builder.Services
    .AddHealthChecks()
    .AddUrlGroup(
        new Uri("http://localhost:5084/health"),
        name: "order-service",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded,
        timeout: TimeSpan.FromSeconds(5))
    .AddUrlGroup(
        new Uri("http://localhost:5170/health"),
        name: "product-service",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded,
        timeout: TimeSpan.FromSeconds(5));

// ── CORS (FE Integration) ──────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:4000")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

// ── Rate Limiting (Global) ────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromSeconds(10),
                PermitLimit = 100
            }));

    options.OnRejected = async (context, _) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            statusCode = 429,
            message = "Too many requests. Please try again later."
        });
    };
});

// ── Unified Swagger UI (Aggregation) ────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "gateway";
    config.Title = "Application API Gateway";
    config.Version = "v1";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUi(settings =>
    {
        settings.Path = "/swagger";
        settings.SwaggerRoutes.Add(new NSwag.AspNetCore.SwaggerUiRoute("Order Service", "/swagger/order/v1/swagger.json"));
        settings.SwaggerRoutes.Add(new NSwag.AspNetCore.SwaggerUiRoute("Product Service", "/swagger/product/v1/swagger.json"));
    });
}

// ── Middleware Pipeline ───────────────────────────────────────────────────────
app.UseCors("DefaultCorsPolicy");
app.UseRouting();
app.UseResponseCompression();
app.UseRequestTimeouts();
app.UseRateLimiter();

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready")
});

app.MapReverseProxy();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🚀 API Gateway started on {Url}", app.Urls.FirstOrDefault() ?? "http://localhost:3000");

app.Run();
