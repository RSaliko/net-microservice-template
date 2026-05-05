using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entities;

namespace OrderService.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(e => new { e.OrderId, e.ProductId });

        builder.Property(e => e.ProductId).IsRequired();
        builder.Property(e => e.Quantity).IsRequired();
        
        // BP #9: Decimal Precision: Use decimal(12,2)
        builder.Property(e => e.UnitPrice).HasPrecision(12, 2).IsRequired();

        builder.HasIndex(e => e.ProductId);
    }
}
