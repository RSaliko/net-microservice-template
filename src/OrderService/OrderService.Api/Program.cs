using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.DataProtection;
using BuildingBlocks.Conventions;
using BuildingBlocks.Extensions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using OrderService.Application;
using OrderService.Persistence;
using OrderService.Infrastructure;
using OrderService.Api.Extensions;
using BuildingBlocks.HealthChecks;
using OrderService.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Logging.ClearProviders();
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());
    
// ── Graceful Shutdown ─────────────────────────────────────────────────────────
builder.Services.Configure<HostOptions>(options =>
{
    options.ShutdownTimeout = TimeSpan.FromSeconds(30);
});

// Data Protection (Persistent Keys)
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "keys")));

// ── Service Layers ────────────────────────────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration, builder.Environment);

builder.Services.AddBuildingBlocks(builder.Configuration);
builder.Services.AddOpenTelemetryObservability(builder.Configuration, "OrderService");

// ── API Controllers ───────────────────────────────────────────────────────────
builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new RouteTokenTransformerConvention(new KebabCaseConvention()));
})
.AddNewtonsoftJson();

// ── Performance & Compression ─────────────────────────────────────────────────
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.SmallestSize);

var app = builder.Build();

// ── Development Setup ─────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
    await app.InitializeDatabaseAsync();
}

// ── Middleware Pipeline ───────────────────────────────────────────────────────
app.UseBuildingBlocks(); 
app.UseRouting();
app.UseResponseCompression();
app.UseStandardHealthChecks();
app.UseRateLimiter();
app.MapControllers();

app.Run();

public partial class Program { }
