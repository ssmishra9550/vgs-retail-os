namespace VGS.RetailOS.Contracts.V1.InventoryManagement.Requests;
public class InitiateStockTransferRequest
{
    public Guid SourceStoreId { get; set; }
    public Guid DestinationStoreId { get; set; }
    public List<StockTransferItemRequest> Items { get; set; } = new();
}
public class StockTransferItemRequest
{
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
}
