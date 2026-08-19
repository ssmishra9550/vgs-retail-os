using VGS.RetailOS.Infrastructure.MasterData.DAC.Entities;
using VGS.RetailOS.Shared.Audit;

namespace VGS.RetailOS.Infrastructure.ProductManagement.DAC.Entities;

public class ProductEntity : IAuditableEntity
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Sku { get; set; }
    public string? Description { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    
    public Guid? CategoryId { get; set; }
    public CategoryEntity? Category { get; set; }
    
    public Guid? BrandId { get; set; }
    public BrandEntity? Brand { get; set; }
    
    public Guid UnitId { get; set; }
    public UnitEntity? Unit { get; set; }
    
    public Guid? TaxId { get; set; }
    public TaxEntity? Tax { get; set; }
    
    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
