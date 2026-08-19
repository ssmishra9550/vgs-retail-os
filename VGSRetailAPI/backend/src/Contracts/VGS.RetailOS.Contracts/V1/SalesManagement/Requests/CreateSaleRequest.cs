namespace VGS.RetailOS.Contracts.V1.SalesManagement.Requests;

public class CreateSaleRequest
{
    public Guid StoreId { get; set; }
    public Guid? CustomerId { get; set; }
    
    public string InvoiceNumber { get; set; } = null!;
    public DateTimeOffset SaleDate { get; set; }
    
    public decimal TotalDiscount { get; set; }
    public decimal TotalTax { get; set; }
    public decimal PaidAmount { get; set; }
    
    public List<SaleItemRequest> Items { get; set; } = new();
}

public class SaleItemRequest
{
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxAmount { get; set; }
}
