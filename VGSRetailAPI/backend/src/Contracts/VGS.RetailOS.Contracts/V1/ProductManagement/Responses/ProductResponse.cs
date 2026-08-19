namespace VGS.RetailOS.Contracts.V1.ProductManagement.Responses;

public class ProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Sku { get; set; }
    public string? Description { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    
    public Guid? BrandId { get; set; }
    public string? BrandName { get; set; }
    
    public Guid UnitId { get; set; }
    public string? UnitName { get; set; }
    
    public Guid? TaxId { get; set; }
    public string? TaxName { get; set; }
    
    public bool IsActive { get; set; }
}
