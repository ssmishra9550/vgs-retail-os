using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VGS.RetailOS.Infrastructure.InventoryManagement.DAC.Entities;

namespace VGS.RetailOS.Infrastructure.InventoryManagement.DAC.Configurations;

public class StockBalanceConfiguration : IEntityTypeConfiguration<StockBalanceEntity>
{
    public void Configure(EntityTypeBuilder<StockBalanceEntity> builder)
    {
        builder.ToTable("StockBalances");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).IsRequired().HasMaxLength(50);
        
        builder.Property(x => x.Quantity).HasColumnType("decimal(18,4)");

        // PostgreSQL concurrency token using xmin
        builder.Property(x => x.Version)
            .IsRowVersion()
            .HasColumnName("xmin")
            .HasColumnType("xid");

        // A store can only have one balance record per product
        builder.HasIndex(x => new { x.TenantId, x.StoreId, x.ProductId })
            .IsUnique();
    }
}
