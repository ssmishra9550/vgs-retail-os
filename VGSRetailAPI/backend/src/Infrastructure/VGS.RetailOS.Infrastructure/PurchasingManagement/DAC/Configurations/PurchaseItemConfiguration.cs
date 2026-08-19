using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VGS.RetailOS.Infrastructure.PurchasingManagement.DAC.Entities;

namespace VGS.RetailOS.Infrastructure.PurchasingManagement.DAC.Configurations;

public class PurchaseItemConfiguration : IEntityTypeConfiguration<PurchaseItemEntity>
{
    public void Configure(EntityTypeBuilder<PurchaseItemEntity> builder)
    {
        builder.ToTable("PurchaseItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).IsRequired().HasMaxLength(50);
        
        builder.Property(x => x.Quantity).HasColumnType("decimal(18,4)");
        builder.Property(x => x.UnitCost).HasColumnType("decimal(18,4)");
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
