using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductService.Domain.Entities;

namespace ProductService.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(j => j.Id);

        builder.Property(j => j.Sku)
            .HasMaxLength(32)
            .IsRequired();
        builder.HasIndex(j => j.Sku)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.Property(j => j.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(j => j.Description)
            .HasMaxLength(1000);

        builder.Property(j => j.UnitPrice)
            .HasColumnType("decimal(12,2)")
            .IsRequired();

        builder.Property(j => j.QuantityStock)
            .IsRequired();

        // RowVersion mapping
        builder.Property(j => j.RowVersion)
            .IsRowVersion();

        // Soft delete index
        builder.HasIndex(j => j.IsDeleted);
    }
}
