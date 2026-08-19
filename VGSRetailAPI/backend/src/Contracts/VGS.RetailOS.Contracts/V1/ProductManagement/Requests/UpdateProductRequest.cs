namespace VGS.RetailOS.Contracts.V1.ProductManagement.Requests;

public class UpdateProductRequest
{
    public string Name { get; set; } = null!;
    public string? Sku { get; set; }
    public string? Description { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    
    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public Guid UnitId { get; set; }
    public Guid? TaxId { get; set; }
    public bool IsActive { get; set; }
}
