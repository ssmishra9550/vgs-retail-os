using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VGS.RetailOS.Infrastructure.SalesManagement.DAC.Entities;

namespace VGS.RetailOS.Infrastructure.SalesManagement.DAC.Configurations;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItemEntity>
{
    public void Configure(EntityTypeBuilder<SaleItemEntity> builder)
    {
        builder.ToTable("SaleItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).IsRequired().HasMaxLength(50);
        
        builder.Property(x => x.Quantity).HasColumnType("decimal(18,4)");
        builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,4)");
        builder.Property(x => x.Discount).HasColumnType("decimal(18,4)");
        builder.Property(x => x.TaxAmount).HasColumnType("decimal(18,4)");
        builder.Property(x => x.Total).HasColumnType("decimal(18,4)");

        // Foreign Key to Product
        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
