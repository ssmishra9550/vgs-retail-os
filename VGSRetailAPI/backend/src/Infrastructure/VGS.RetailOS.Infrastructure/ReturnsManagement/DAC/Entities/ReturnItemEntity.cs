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
