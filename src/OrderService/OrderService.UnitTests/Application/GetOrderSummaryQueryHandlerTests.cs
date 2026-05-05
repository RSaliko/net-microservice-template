using AutoFixture;
using FluentAssertions;
using NSubstitute;
using OrderService.Application.Clients;
using OrderService.Application.Features.Orders.DTOs;
using OrderService.Application.Features.Orders.Queries.GetOrderSummary;
using BuildingBlocks.Models;

namespace OrderService.UnitTests.Application;

public class GetOrderSummaryQueryHandlerTests
{
    private readonly IProductServiceClient _productServiceClient;
    private readonly GetOrderSummaryQueryHandler _handler;
    private readonly Fixture _fixture;

    public GetOrderSummaryQueryHandlerTests()
    {
        _productServiceClient = Substitute.For<IProductServiceClient>();
        var cacheService = Substitute.For<BuildingBlocks.Caching.ICacheService>();
        _handler = new GetOrderSummaryQueryHandler(_productServiceClient, cacheService);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task Handle_Should_ReturnSummary_When_ProductServiceReturnsData()
    {
        // Arrange (FIRST: Independent, AAA: Arrange)
        var query = new GetOrderSummaryQuery();
        var products = _fixture.CreateMany<ExternalProductDto>(5).ToList();
        var apiResponse = new ProductServiceResponse(products, 200, "Success", true);

        _productServiceClient.GetProductsAsync().Returns(apiResponse);

        // Act (AAA: Act)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (AAA: Assert)
        result.Should().NotBeNull();
        result.TotalProducts.Should().Be(5);
        result.RecentProducts.Should().HaveCount(3);
    }
}
