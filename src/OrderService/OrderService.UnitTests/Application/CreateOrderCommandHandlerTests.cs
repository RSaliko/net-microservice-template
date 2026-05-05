using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using OrderService.Application.Common.Interfaces;
using OrderService.Application.Features.Orders.Commands.CreateOrder;
using OrderService.Domain.Entities;

namespace OrderService.UnitTests.Application;

public class CreateOrderCommandHandlerTests
{
    private readonly IApplicationDbContext _context;
    private readonly CreateOrderCommandHandler _handler;
    private readonly Fixture _fixture;

    public CreateOrderCommandHandlerTests()
    {
        _context = Substitute.For<IApplicationDbContext>();
        var publishEndpoint = Substitute.For<MassTransit.IPublishEndpoint>();
        _handler = new CreateOrderCommandHandler(_context, publishEndpoint);
        _fixture = new Fixture();
        
        // Mock DbSet
        var mockSet = Substitute.For<DbSet<Order>, IQueryable<Order>>();
        _context.Orders.Returns(mockSet);
    }

    [Fact]
    public async Task Handle_Should_CreateOrder_When_CommandIsValid()
    {
        // Arrange
        var items = _fixture.CreateMany<CreateOrderItemCommand>(2).ToList();
        var command = new CreateOrderCommand("ORD-TEST", "Receiver", "090", "test@test.com", "Note", items);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _context.Orders.Received(1).Add(Arg.Is<Order>(o => o.OrderCode == "ORD-TEST"));
        await _context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
