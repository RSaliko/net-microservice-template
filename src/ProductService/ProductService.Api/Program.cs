using System.IO;
using Microsoft.AspNetCore.DataProtection;
using BuildingBlocks.Conventions;
using BuildingBlocks.Extensions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using ProductService.Application;
using ProductService.Persistence;
using ProductService.Api.Extensions;
using BuildingBlocks.HealthChecks;
using ProductService.Persistence.Contexts;
using ProductService.Persistence.Data;
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
builder.Services.AddApiServices(builder.Configuration, builder.Environment);

builder.Services.AddBuildingBlocks(builder.Configuration);
builder.Services.AddOpenTelemetryObservability(builder.Configuration, "ProductService");
builder.Services.AddJwtAuthentication(builder.Configuration);

// ── API Controllers ───────────────────────────────────────────────────────────
builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new RouteTokenTransformerConvention(new KebabCaseConvention()));
})
.AddNewtonsoftJson();

// ── Performance & Compression ─────────────────────────────────────────────────
builder.Services.AddResponseCompression(options => { options.EnableForHttps = true; });

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
app.UseAuthorization();
app.MapControllers();

app.Run();
