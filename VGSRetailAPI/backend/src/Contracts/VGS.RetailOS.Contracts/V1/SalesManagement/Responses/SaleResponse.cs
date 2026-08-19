namespace VGS.RetailOS.Contracts.V1.SalesManagement.Responses;

public class SaleResponse
{
    public Guid Id { get; set; }
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
    
    public List<SaleItemResponse> Items { get; set; } = new();
}

public class SaleItemResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
}
