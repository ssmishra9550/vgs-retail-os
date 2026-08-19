namespace VGS.RetailOS.Contracts.V1.PurchasingManagement.Responses;

public class PurchaseResponse
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public Guid SupplierId { get; set; }
    public string InvoiceNumber { get; set; } = null!;
    public DateTimeOffset InvoiceDate { get; set; }
    public string Status { get; set; } = null!;
    
    public decimal SubTotal { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalTax { get; set; }
    public decimal GrandTotal { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }

    public List<PurchaseItemResponse> Items { get; set; } = new List<PurchaseItemResponse>();
}

public class PurchaseItemResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
}
