using VGS.RetailOS.Infrastructure.ProductManagement.DAC.Entities;

namespace VGS.RetailOS.Infrastructure.PurchasingManagement.DAC.Entities;

public class PurchaseItemEntity
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = null!;
    
    public Guid PurchaseId { get; set; }
    public Guid ProductId { get; set; }
    
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }

    public PurchaseEntity? Purchase { get; set; }
    public ProductEntity? Product { get; set; }
}
