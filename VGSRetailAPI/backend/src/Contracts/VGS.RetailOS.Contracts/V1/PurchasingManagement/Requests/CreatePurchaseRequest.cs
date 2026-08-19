using System.ComponentModel.DataAnnotations;

namespace VGS.RetailOS.Contracts.V1.PurchasingManagement.Requests;

public class CreatePurchaseRequest
{
    [Required]
    public Guid StoreId { get; set; }
    
    [Required]
    public Guid SupplierId { get; set; }

    [Required]
    [MaxLength(100)]
    public string InvoiceNumber { get; set; } = null!;
    
    [Required]
    public DateTimeOffset InvoiceDate { get; set; }

    public decimal TotalDiscount { get; set; }
    public decimal TotalTax { get; set; }
    
    [Required]
    public List<PurchaseItemRequest> Items { get; set; } = new List<PurchaseItemRequest>();
}

public class PurchaseItemRequest
{
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    public decimal Quantity { get; set; }
    
    [Required]
    public decimal UnitCost { get; set; }
    
    public decimal Discount { get; set; }
    public decimal TaxAmount { get; set; }
}
