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
    private readonly OrderService.Application.Common.Interfaces.IOrderRepository _orderRepository;
    private readonly BuildingBlocks.Data.IUnitOfWork _unitOfWork;
    private readonly CreateOrderCommandHandler _handler;
    private readonly Fixture _fixture;

    public CreateOrderCommandHandlerTests()
    {
        _orderRepository = Substitute.For<OrderService.Application.Common.Interfaces.IOrderRepository>();
        _unitOfWork = Substitute.For<BuildingBlocks.Data.IUnitOfWork>();
        var publishEndpoint = Substitute.For<MassTransit.IPublishEndpoint>();
        _handler = new CreateOrderCommandHandler(_orderRepository, _unitOfWork, publishEndpoint);
        _fixture = new Fixture();
        
        // Mock repository AddAsync behavior
        _orderRepository.AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>())
            .Returns(ci => Task.FromResult(ci.ArgAt<Order>(0)));
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
        _orderRepository.Received(1).AddAsync(Arg.Is<Order>(o => o.OrderCode == "ORD-TEST"), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
