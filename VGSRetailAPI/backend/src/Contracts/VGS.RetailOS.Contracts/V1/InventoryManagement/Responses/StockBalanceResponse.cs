namespace VGS.RetailOS.Contracts.V1.InventoryManagement.Responses;

public class StockBalanceResponse
{
    public Guid StoreId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
}
