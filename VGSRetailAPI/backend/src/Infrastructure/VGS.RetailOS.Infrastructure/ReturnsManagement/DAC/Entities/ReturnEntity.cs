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
