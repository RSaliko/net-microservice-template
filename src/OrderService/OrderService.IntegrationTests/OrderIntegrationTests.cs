using BuildingBlocks.TestUtils;
using OrderService.Persistence.Contexts;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace OrderService.IntegrationTests;

public class OrderIntegrationTests : BaseIntegrationTest<Program, OrderServiceDbContext>
{
    [Fact]
    public async Task Database_Should_Be_Accessible()
    {
        // Arrange & Act
        var canConnect = await DbContext.Database.CanConnectAsync();

        // Assert
        canConnect.Should().BeTrue();
    }
}
