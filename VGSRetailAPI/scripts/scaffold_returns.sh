#!/bin/bash
BASE="/Users/sauravmishra/VGSRetail/VGSRetailAPI/backend/src"
INFRA_RET="$BASE/Infrastructure/VGS.RetailOS.Infrastructure/ReturnsManagement"
MOD_RET="$BASE/Modules/VGS.RetailOS.Modules/ReturnsManagement"
CONTRACTS="$BASE/Contracts/VGS.RetailOS.Contracts/V1/ReturnsManagement"
API="$BASE/ApiHost/VGS.RetailOS.ApiHost/Controllers/V1/ReturnsManagement"

mkdir -p "$INFRA_RET/DAC/Entities" "$INFRA_RET/DAC"
mkdir -p "$MOD_RET/Return/BO" "$MOD_RET/Return/IDAC" "$MOD_RET/Return/IBL" "$MOD_RET/Return/BL"
mkdir -p "$CONTRACTS/Requests" "$CONTRACTS/Responses"
mkdir -p "$API"

# Entities
cat << 'EOT' > "$INFRA_RET/DAC/Entities/ReturnEntity.cs"
using VGS.RetailOS.Shared.Audit;
using VGS.RetailOS.Infrastructure.Store.DAC.Entities;
using VGS.RetailOS.Infrastructure.CustomerManagement.DAC.Entities;
using VGS.RetailOS.Infrastructure.SupplierManagement.DAC.Entities;
using VGS.RetailOS.Infrastructure.SalesManagement.DAC.Entities;
using VGS.RetailOS.Infrastructure.PurchasingManagement.DAC.Entities;

namespace VGS.RetailOS.Infrastructure.ReturnsManagement.DAC.Entities;

public class ReturnEntity : IAuditableEntity
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = null!;
    public string ReturnNumber { get; set; } = null!;
    public string ReturnType { get; set; } = null!; // CustomerReturn or SupplierReturn
    
    public Guid StoreId { get; set; }
    public StoreEntity? Store { get; set; }
    
    public Guid? CustomerId { get; set; }
    public CustomerEntity? Customer { get; set; }
    
    public Guid? SupplierId { get; set; }
    public SupplierEntity? Supplier { get; set; }
    
    public Guid? SaleId { get; set; }
    public SaleEntity? Sale { get; set; }
    
    public Guid? PurchaseId { get; set; }
    public PurchaseEntity? Purchase { get; set; }
    
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Completed"; // Completed, PendingRefund
    
    public ICollection<ReturnItemEntity> Items { get; set; } = new List<ReturnItemEntity>();

    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
EOT

cat << 'EOT' > "$INFRA_RET/DAC/Entities/ReturnItemEntity.cs"
using VGS.RetailOS.Infrastructure.ProductManagement.DAC.Entities;
namespace VGS.RetailOS.Infrastructure.ReturnsManagement.DAC.Entities;

public class ReturnItemEntity
{
    public Guid Id { get; set; }
    public Guid ReturnId { get; set; }
    public ReturnEntity? Return { get; set; }
    
    public Guid ProductId { get; set; }
    public ProductEntity? Product { get; set; }
    
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Reason { get; set; }
}
EOT

# AppDbContext Patch
sed -i '' '/public DbSet<VGS.RetailOS.Infrastructure.ExpensesManagement.Entities.ExpenseEntity> Expenses { get; set; } = default!;/a\
    public DbSet<VGS.RetailOS.Infrastructure.ReturnsManagement.DAC.Entities.ReturnEntity> Returns { get; set; } = default!;\
    public DbSet<VGS.RetailOS.Infrastructure.ReturnsManagement.DAC.Entities.ReturnItemEntity> ReturnItems { get; set; } = default!;\
' $BASE/Infrastructure/VGS.RetailOS.Infrastructure/Data/AppDbContext.cs

sed -i '' '/builder.Entity<VGS.RetailOS.Infrastructure.ExpensesManagement.Entities.ExpenseEntity>()/a\
\
        builder.Entity<VGS.RetailOS.Infrastructure.ReturnsManagement.DAC.Entities.ReturnEntity>()\
            .HasQueryFilter(r => CurrentTenantId == null || r.TenantId == CurrentTenantId);\
' $BASE/Infrastructure/VGS.RetailOS.Infrastructure/Data/AppDbContext.cs

