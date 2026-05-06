using BuildingBlocks.Caching;
using BuildingBlocks.Data;
using FluentAssertions;
using MassTransit;
using NSubstitute;
using ProductService.Application.Common.Interfaces;
using ProductService.Application.Features.Products.Commands.CreateProduct;
using ProductService.Application.Features.Products.DTOs;
using ProductService.Application.Features.Products.Mappers;
using ProductService.Application.Features.Products.Queries.GetProduct;
using ProductService.Domain.Entities;

namespace ProductService.UnitTests.Application;

public class ProductHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ICacheService _cacheService;
    private readonly ProductMapper _mapper;

    public ProductHandlerTests()
    {
        _productRepository = Substitute.For<IProductRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _cacheService = Substitute.For<ICacheService>();
        _mapper = new ProductMapper();

        _productRepository.AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(ci => Task.FromResult(ci.ArgAt<Product>(0)));
    }

    [Fact]
    public async Task CreateProduct_Should_Persist_Publish_And_Cache_Product()
    {
        var handler = new CreateProductCommandHandler(_productRepository, _unitOfWork, _publishEndpoint, _mapper, _cacheService);
        var command = new CreateProductCommand("SKU-001", "Keyboard", "Mechanical", 120.50m, 8);

        var productId = await handler.Handle(command, CancellationToken.None);

        productId.Should().NotBe(Guid.Empty);
        await _productRepository.Received(1).AddAsync(Arg.Is<Product>(product => product.Sku == "SKU-001" && product.Name == "Keyboard"), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _publishEndpoint.Received(1).Publish(Arg.Any<BuildingBlocks.Contracts.Events.ProductCreatedEvent>(), Arg.Any<CancellationToken>());
        await _cacheService.Received(1).SetAsync(Arg.Is<string>(key => key == $"product:{productId}"), Arg.Any<ProductDto>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetProduct_Should_Return_Cached_Product_When_Present()
    {
        var cachedProduct = new ProductDto
        {
            Id = Guid.NewGuid(),
            Sku = "SKU-CACHED",
            Name = "Cached Product",
            Status = "Active",
            UnitPrice = 99.99m,
            QuantityStock = 3
        };

        _cacheService.GetAsync<ProductDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(cachedProduct);

        var handler = new GetProductQueryHandler(_productRepository, _mapper, _cacheService);
        var result = await handler.Handle(new GetProductQuery(cachedProduct.Id), CancellationToken.None);

        result.Should().BeEquivalentTo(cachedProduct);
        await _productRepository.DidNotReceive().FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetProduct_Should_Load_And_Cache_Product_When_Not_In_Cache()
    {
        var product = new Product("SKU-002", "Mouse", "Wireless", 45m, 12);
        _cacheService.GetAsync<ProductDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ProductDto?)null);
        _productRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(product);

        var handler = new GetProductQueryHandler(_productRepository, _mapper, _cacheService);
        var result = await handler.Handle(new GetProductQuery(product.Id), CancellationToken.None);

        result.Id.Should().Be(product.Id);
        result.Sku.Should().Be("SKU-002");
        await _productRepository.Received(1).FindByIdAsync(product.Id, Arg.Any<CancellationToken>());
        await _cacheService.Received(1).SetAsync(Arg.Is<string>(key => key == $"product:{product.Id}"), Arg.Any<ProductDto>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>());
    }
}