using BuildingBlocks.Caching;
using BuildingBlocks.Data;
using BuildingBlocks.Models;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore.Query;
using NSubstitute;
using ProductService.Application.Common.Interfaces;
using ProductService.Application.Features.Products.Commands.CreateProduct;
using ProductService.Application.Features.Products.Commands.DeactivateAllProducts;
using ProductService.Application.Features.Products.DTOs;
using ProductService.Application.Features.Products.Mappers;
using ProductService.Application.Features.Products.Queries.GetProducts;
using ProductService.Application.Features.Products.Queries.GetProduct;
using ProductService.Domain.Entities;
using System.Linq.Expressions;

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

    [Fact]
    public async Task GetProducts_Should_Filter_And_Sort_Results()
    {
        var products = new List<Product>
        {
            new("SKU-100", "Keyboard", null, 100m, 5) { CreatedAt = DateTimeOffset.UtcNow.AddDays(-2) },
            new("SKU-200", "Keycap", null, 20m, 15) { CreatedAt = DateTimeOffset.UtcNow.AddDays(-1) },
            new("SKU-300", "Mouse", null, 50m, 8) { CreatedAt = DateTimeOffset.UtcNow }
        };

        _productRepository.Query().Returns(new TestAsyncEnumerable<Product>(products));

        var handler = new GetProductsQueryHandler(_productRepository, _mapper);
        var result = await handler.Handle(new GetProductsQuery(
            new PagingParams(1, 10), 
            new SortingParams("name"), 
            new FilteringParams("Key")), CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.Select(item => item.Name).Should().ContainInOrder("Keyboard", "Keycap");
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task DeactivateAllProducts_Should_Delegate_To_Repository()
    {
        _productRepository.DeactivateAllActiveAsync(Arg.Any<CancellationToken>()).Returns(3);

        var handler = new DeactivateAllProductsCommandHandler(_productRepository);
        var result = await handler.Handle(new DeactivateAllProductsCommand(), CancellationToken.None);

        result.Should().Be(3);
        await _productRepository.Received(1).DeactivateAllActiveAsync(Arg.Any<CancellationToken>());
    }
}

internal sealed class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable)
    {
    }

    public TestAsyncEnumerable(Expression expression) : base(expression)
    {
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new TestAsyncEnumerator<T>(this.ToList().GetEnumerator());

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

internal sealed class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }

    public T Current => _inner.Current;

    public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());
}

internal sealed class TestAsyncQueryProvider<T> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    public TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<T>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression);

    public object? Execute(Expression expression) => _inner.Execute(expression);

    public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var result = Execute(expression);
        var taskResultType = typeof(TResult).GetGenericArguments()[0];
        var fromResultMethod = typeof(Task)
            .GetMethods()
            .Single(method => method.Name == nameof(Task.FromResult) && method.IsGenericMethodDefinition)
            .MakeGenericMethod(taskResultType);

        return (TResult)fromResultMethod.Invoke(null, [result])!;
    }
}