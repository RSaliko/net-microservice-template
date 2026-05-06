using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Generic;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;
using Xunit;

namespace BuildingBlocks.TestUtils;

public abstract class BaseIntegrationTest<TProgram, TDbContext> : IAsyncLifetime
    where TProgram : class
    where TDbContext : DbContext
{
    protected readonly MsSqlContainer MsSqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    protected readonly RabbitMqContainer RabbitMqContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management")
        .Build();

    protected WebApplicationFactory<TProgram> Factory = null!;
    protected IServiceScope Scope = null!;
    protected TDbContext DbContext = null!;

    public virtual async Task InitializeAsync()
    {
        await Task.WhenAll(MsSqlContainer.StartAsync(), RabbitMqContainer.StartAsync());

        Factory = new WebApplicationFactory<TProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Jwt:Secret"] = "integration-test-secret-key-1234567890",
                        ["Jwt:Issuer"] = "TicketFlow",
                        ["Jwt:Audience"] = "TicketFlowUI"
                    });
                });

                builder.ConfigureServices(services =>
                {
                    // Remove existing DB context registration
                    services.RemoveAll(typeof(DbContextOptions<TDbContext>));
                    services.RemoveAll(typeof(TDbContext));

                    // Add DB context using Testcontainer connection string
                    services.AddDbContext<TDbContext>(options =>
                        options.UseSqlServer(MsSqlContainer.GetConnectionString()));

                    // RabbitMQ override would happen here if we used a standard configuration pattern
                    // For now, we assume the host can be overridden via environment variables or options
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
        await Task.WhenAll(MsSqlContainer.DisposeAsync().AsTask(), RabbitMqContainer.DisposeAsync().AsTask());
    }
}
