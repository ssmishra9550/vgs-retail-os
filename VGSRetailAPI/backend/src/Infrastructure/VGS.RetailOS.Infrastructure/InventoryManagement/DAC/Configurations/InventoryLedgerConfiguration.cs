using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VGS.RetailOS.Infrastructure.InventoryManagement.DAC.Entities;

namespace VGS.RetailOS.Infrastructure.InventoryManagement.DAC.Configurations;

public class InventoryLedgerConfiguration : IEntityTypeConfiguration<InventoryLedgerEntity>
{
    public void Configure(EntityTypeBuilder<InventoryLedgerEntity> builder)
    {
        builder.ToTable("InventoryLedger");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).IsRequired().HasMaxLength(50);
        
        builder.Property(x => x.ChangeQuantity).HasColumnType("decimal(18,4)");
        builder.Property(x => x.BalanceAfter).HasColumnType("decimal(18,4)");
        
        builder.Property(x => x.TransactionType).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Reason).HasMaxLength(500);

        // Index for fast querying of a product's history at a store
        builder.HasIndex(x => new { x.TenantId, x.StoreId, x.ProductId });
        
        // Index for looking up all ledger entries related to a specific transaction (e.g. Purchase Receipt)
        builder.HasIndex(x => new { x.TenantId, x.ReferenceId });
    }
}
