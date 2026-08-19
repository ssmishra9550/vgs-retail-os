using VGS.RetailOS.Infrastructure.SupplierManagement.DAC.Entities;

namespace VGS.RetailOS.Infrastructure.PurchasingManagement.DAC.Entities;

public class PurchaseEntity
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = null!;
    public Guid StoreId { get; set; }
    public Guid SupplierId { get; set; }
    
    public string InvoiceNumber { get; set; } = null!;
    public DateTimeOffset InvoiceDate { get; set; }
    
    public string Status { get; set; } = null!; // Draft, Received, Cancelled
    
    public decimal SubTotal { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalTax { get; set; }
    public decimal GrandTotal { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }

    public ICollection<PurchaseItemEntity> Items { get; set; } = new List<PurchaseItemEntity>();
    public SupplierEntity? Supplier { get; set; }
}
