using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VGS.RetailOS.Infrastructure.PurchasingManagement.DAC.Entities;

namespace VGS.RetailOS.Infrastructure.PurchasingManagement.DAC.Configurations;

public class PurchaseConfiguration : IEntityTypeConfiguration<PurchaseEntity>
{
    public void Configure(EntityTypeBuilder<PurchaseEntity> builder)
    {
        builder.ToTable("Purchases");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.InvoiceNumber).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Status).IsRequired().HasMaxLength(50);
        
        builder.Property(x => x.SubTotal).HasColumnType("decimal(18,4)");
        builder.Property(x => x.TotalDiscount).HasColumnType("decimal(18,4)");
        builder.Property(x => x.TotalTax).HasColumnType("decimal(18,4)");
        builder.Property(x => x.GrandTotal).HasColumnType("decimal(18,4)");

        // A supplier should not have duplicate invoice numbers in the same tenant
        builder.HasIndex(x => new { x.TenantId, x.SupplierId, x.InvoiceNumber }).IsUnique();

        // Foreign Key to Supplier
        builder.HasOne(x => x.Supplier)
            .WithMany()
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        // One-to-Many with PurchaseItems
        builder.HasMany(x => x.Items)
            .WithOne(x => x.Purchase)
            .HasForeignKey(x => x.PurchaseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
