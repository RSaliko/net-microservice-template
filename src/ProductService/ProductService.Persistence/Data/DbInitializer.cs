using BuildingBlocks.Data;
using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Persistence.Contexts;
using Bogus;

namespace ProductService.Persistence.Data;

public class ProductServiceDbInitializer(ProductServiceDbContext context) : IDbInitializer
{
    private readonly ProductServiceDbContext _context = context;

    public async Task InitializeAsync()
    {
        await _context.Database.MigrateAsync();
        await SeedAsync();
    }

    public async Task SeedAsync()
    {
        if (await _context.Products.AnyAsync()) return;

        // BP: Using Bogus for realistic seed data
        var productFaker = new Faker<Product>()
            .CustomInstantiator(f => new Product(
                f.Commerce.Ean13(),
                f.Commerce.ProductName(),
                f.Commerce.ProductDescription(),
                f.Finance.Amount(10, 2000),
                f.Random.Int(0, 100)
            ));

        var products = productFaker.Generate(20);

        foreach (var product in products)
        {
            product.Activate();
        }

        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();
    }
}
