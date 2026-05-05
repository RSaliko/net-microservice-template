using BuildingBlocks.Data;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.ValueObjects;
using OrderService.Persistence.Contexts;
using Bogus;

namespace OrderService.Persistence.Data;

public class OrderServiceDbInitializer(OrderServiceDbContext context) : IDbInitializer
{
    private readonly OrderServiceDbContext _context = context;

    public async Task InitializeAsync()
    {
        await _context.Database.MigrateAsync();
        await SeedAsync();
    }

    public async Task SeedAsync()
    {
        if (await _context.Orders.AnyAsync()) return;

        var orderFaker = new Faker<Order>()
            .CustomInstantiator(f => new Order(
                $"ORD-{f.Date.Recent().Year}-{f.Random.Number(1000, 9999)}",
                f.Name.FullName(),
                f.Phone.PhoneNumber(),
                f.Internet.Email(),
                f.Lorem.Sentence()
            ));

        var orders = orderFaker.Generate(10);

        var f = new Faker();
        foreach (var order in orders)
        {
            // Add some items
            var itemCount = f.Random.Int(1, 3);
            for (int i = 0; i < itemCount; i++)
            {
                order.AddItem(Guid.NewGuid(), f.Random.Int(1, 5), (decimal)f.Random.Double() * 500);
            }

            order.SetAddress(new Address(
                f.Address.StreetAddress(),
                f.Address.City(),
                f.Address.State(),
                f.Address.ZipCode(),
                f.Address.Country()
            ));

            if (f.Random.Double() > 0.3) order.Activate();
        }

        await _context.Orders.AddRangeAsync(orders);
        await _context.SaveChangesAsync();
    }
}
