using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Generic;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Xunit;

namespace BuildingBlocks.TestUtils;

/// <summary>
/// BP #12: Base class for integration tests using Testcontainers (PostgreSQL + RabbitMQ).
/// Each test class gets isolated, real infrastructure containers.
/// </summary>
public abstract class BaseIntegrationTest<TProgram, TDbContext> : IAsyncLifetime
    where TProgram : class
    where TDbContext : DbContext
{
    protected readonly PostgreSqlContainer PostgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    protected readonly RabbitMqContainer RabbitMqContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management")
        .Build();

    protected WebApplicationFactory<TProgram> Factory = null!;
    protected IServiceScope Scope = null!;
    protected TDbContext DbContext = null!;

    public virtual async Task InitializeAsync()
    {
        await Task.WhenAll(PostgreSqlContainer.StartAsync(), RabbitMqContainer.StartAsync());

        Factory = new WebApplicationFactory<TProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["RabbitMQ:Host"] = RabbitMqContainer.GetConnectionString()
                    });
                });

                builder.ConfigureServices(services =>
                {
                    // Remove existing DB context registration
                    services.RemoveAll(typeof(DbContextOptions<TDbContext>));
                    services.RemoveAll(typeof(TDbContext));

                    // Add DB context using PostgreSQL Testcontainer
                    services.AddDbContext<TDbContext>(options =>
                        options.UseNpgsql(PostgreSqlContainer.GetConnectionString()));
                });
            });

        Scope = Factory.Services.CreateScope();
        DbContext = Scope.ServiceProvider.GetRequiredService<TDbContext>();
        
        await DbContext.Database.EnsureCreatedAsync();
    }

    public virtual async Task DisposeAsync()
    {
        Scope.Dispose();
        await Factory.DisposeAsync();
        await Task.WhenAll(PostgreSqlContainer.DisposeAsync().AsTask(), RabbitMqContainer.DisposeAsync().AsTask());
    }
}
