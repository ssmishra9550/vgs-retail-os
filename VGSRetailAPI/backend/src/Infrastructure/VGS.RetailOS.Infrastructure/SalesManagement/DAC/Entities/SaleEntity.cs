using VGS.RetailOS.Infrastructure.CustomerManagement.DAC.Entities;

namespace VGS.RetailOS.Infrastructure.SalesManagement.DAC.Entities;

public class SaleEntity
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = null!;
    public Guid StoreId { get; set; }
    public Guid? CustomerId { get; set; }
    
    public string InvoiceNumber { get; set; } = null!;
    public DateTimeOffset SaleDate { get; set; }
    
    public string Status { get; set; } = null!; // Draft, Completed, Cancelled
    
    public decimal SubTotal { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalTax { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PaidAmount { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }

    public ICollection<SaleItemEntity> Items { get; set; } = new List<SaleItemEntity>();
    public CustomerEntity? Customer { get; set; }
}
