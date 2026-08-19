#!/bin/bash
BASE="/Users/sauravmishra/VGSRetail/VGSRetailAPI/backend/src"
INFRA_INV="$BASE/Infrastructure/VGS.RetailOS.Infrastructure/InventoryManagement"
MOD_INV="$BASE/Modules/VGS.RetailOS.Modules/InventoryManagement"
CONTRACTS="$BASE/Contracts/VGS.RetailOS.Contracts/V1/InventoryManagement"
API="$BASE/ApiHost/VGS.RetailOS.ApiHost/Controllers/V1/InventoryManagement"

mkdir -p "$INFRA_INV/DAC/Entities" "$INFRA_INV/DAC"
mkdir -p "$MOD_INV/StockTransfer/BO" "$MOD_INV/StockTransfer/IDAC" "$MOD_INV/StockTransfer/IBL" "$MOD_INV/StockTransfer/BL"
mkdir -p "$CONTRACTS/Requests" "$CONTRACTS/Responses"
mkdir -p "$API"

# Entities
cat << 'EOT' > "$INFRA_INV/DAC/Entities/StockTransferEntity.cs"
using VGS.RetailOS.Shared.Audit;
using VGS.RetailOS.Infrastructure.Store.DAC.Entities;

namespace VGS.RetailOS.Infrastructure.InventoryManagement.DAC.Entities;

public class StockTransferEntity : IAuditableEntity
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = null!;
    public string TransferNumber { get; set; } = null!;
    public Guid SourceStoreId { get; set; }
    public StoreEntity? SourceStore { get; set; }
    public Guid DestinationStoreId { get; set; }
    public StoreEntity? DestinationStore { get; set; }
    
    public string Status { get; set; } = "Initiated"; // Initiated, InTransit, Received, Cancelled
    public DateTimeOffset? ShippedAt { get; set; }
    public DateTimeOffset? ReceivedAt { get; set; }
    
    public ICollection<StockTransferItemEntity> Items { get; set; } = new List<StockTransferItemEntity>();

    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
EOT

cat << 'EOT' > "$INFRA_INV/DAC/Entities/StockTransferItemEntity.cs"
using VGS.RetailOS.Infrastructure.ProductManagement.DAC.Entities;
namespace VGS.RetailOS.Infrastructure.InventoryManagement.DAC.Entities;

public class StockTransferItemEntity
{
    public Guid Id { get; set; }
    public Guid StockTransferId { get; set; }
    public StockTransferEntity? StockTransfer { get; set; }
    
    public Guid ProductId { get; set; }
    public ProductEntity? Product { get; set; }
    
    public decimal Quantity { get; set; }
    public decimal? ReceivedQuantity { get; set; }
}
EOT

# Contracts
cat << 'EOT' > "$CONTRACTS/Requests/InitiateStockTransferRequest.cs"
namespace VGS.RetailOS.Contracts.V1.InventoryManagement.Requests;
public class InitiateStockTransferRequest
{
    public Guid SourceStoreId { get; set; }
    public Guid DestinationStoreId { get; set; }
    public List<StockTransferItemRequest> Items { get; set; } = new();
}
public class StockTransferItemRequest
{
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
}
EOT

cat << 'EOT' > "$CONTRACTS/Requests/ReceiveStockTransferRequest.cs"
namespace VGS.RetailOS.Contracts.V1.InventoryManagement.Requests;
public class ReceiveStockTransferRequest
{
    public List<StockTransferReceiveItemRequest> Items { get; set; } = new();
}
public class StockTransferReceiveItemRequest
{
    public Guid StockTransferItemId { get; set; }
    public decimal ReceivedQuantity { get; set; }
}
EOT

# AppDbContext Patch
sed -i '' '/public DbSet<VGS.RetailOS.Infrastructure.InventoryManagement.DAC.Entities.StockBalanceEntity> StockBalances { get; set; } = default!;/a\
    public DbSet<VGS.RetailOS.Infrastructure.InventoryManagement.DAC.Entities.StockTransferEntity> StockTransfers { get; set; } = default!;\
    public DbSet<VGS.RetailOS.Infrastructure.InventoryManagement.DAC.Entities.StockTransferItemEntity> StockTransferItems { get; set; } = default!;\
' $BASE/Infrastructure/VGS.RetailOS.Infrastructure/Data/AppDbContext.cs

sed -i '' '/builder.Entity<VGS.RetailOS.Infrastructure.InventoryManagement.DAC.Entities.StockBalanceEntity>()/a\
\
        builder.Entity<VGS.RetailOS.Infrastructure.InventoryManagement.DAC.Entities.StockTransferEntity>()\
            .HasQueryFilter(s => CurrentTenantId == null || s.TenantId == CurrentTenantId);\
' $BASE/Infrastructure/VGS.RetailOS.Infrastructure/Data/AppDbContext.cs

