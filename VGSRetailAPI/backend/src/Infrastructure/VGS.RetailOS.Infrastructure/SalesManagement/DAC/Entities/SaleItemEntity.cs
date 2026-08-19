using VGS.RetailOS.Infrastructure.ProductManagement.DAC.Entities;

namespace VGS.RetailOS.Infrastructure.SalesManagement.DAC.Entities;

public class SaleItemEntity
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = null!;
    
    public Guid SaleId { get; set; }
    public Guid ProductId { get; set; }
    
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }

    public SaleEntity? Sale { get; set; }
    public ProductEntity? Product { get; set; }
}
