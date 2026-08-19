using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VGS.RetailOS.Infrastructure.SalesManagement.DAC.Entities;

namespace VGS.RetailOS.Infrastructure.SalesManagement.DAC.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<SaleEntity>
{
    public void Configure(EntityTypeBuilder<SaleEntity> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.InvoiceNumber).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Status).IsRequired().HasMaxLength(50);
        
        builder.Property(x => x.SubTotal).HasColumnType("decimal(18,4)");
        builder.Property(x => x.TotalDiscount).HasColumnType("decimal(18,4)");
        builder.Property(x => x.TotalTax).HasColumnType("decimal(18,4)");
        builder.Property(x => x.GrandTotal).HasColumnType("decimal(18,4)");
        builder.Property(x => x.PaidAmount).HasColumnType("decimal(18,4)");

        // A tenant should not have duplicate invoice numbers
        builder.HasIndex(x => new { x.TenantId, x.InvoiceNumber }).IsUnique();

        // Foreign Key to Customer
        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // One-to-Many with SaleItems
        builder.HasMany(x => x.Items)
            .WithOne(x => x.Sale)
            .HasForeignKey(x => x.SaleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
