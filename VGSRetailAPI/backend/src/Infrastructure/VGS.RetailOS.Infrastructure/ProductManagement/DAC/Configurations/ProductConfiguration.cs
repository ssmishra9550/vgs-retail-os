using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VGS.RetailOS.Infrastructure.ProductManagement.DAC.Entities;

namespace VGS.RetailOS.Infrastructure.ProductManagement.DAC.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<ProductEntity>
{
    public void Configure(EntityTypeBuilder<ProductEntity> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Sku)
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.PurchasePrice)
            .HasColumnType("decimal(18,4)");

        builder.Property(x => x.SellingPrice)
            .HasColumnType("decimal(18,4)");

        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Brand)
            .WithMany()
            .HasForeignKey(x => x.BrandId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Unit)
            .WithMany()
            .HasForeignKey(x => x.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Tax)
            .WithMany()
            .HasForeignKey(x => x.TaxId)
            .OnDelete(DeleteBehavior.Restrict);

        // A tenant cannot have two active products with the exact same SKU (if SKU is provided)
        builder.HasIndex(x => new { x.TenantId, x.Sku })
            .IsUnique()
            .HasFilter("\"Sku\" IS NOT NULL");
    }
}
