using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entities;

namespace OrderService.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.OrderCode).IsRequired().HasMaxLength(32);
        builder.Property(e => e.ReceiverName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Phone).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Note).HasMaxLength(1000);
        
        // BP #9: Soft delete index must include filter
        builder.HasIndex(e => e.OrderCode)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasMany(e => e.Items)
            .WithOne(e => e.Order)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false); // Suppress EF Core global filter warning

        builder.OwnsOne(e => e.Address);
    }
}
