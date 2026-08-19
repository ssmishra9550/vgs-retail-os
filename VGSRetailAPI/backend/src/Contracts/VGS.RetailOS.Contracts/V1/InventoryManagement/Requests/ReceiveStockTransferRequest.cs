namespace VGS.RetailOS.Contracts.V1.InventoryManagement.Requests;
public class ReceiveStockTransferRequest
{
    public List<StockTransferReceiveItemRequest> Items { get; set; } = new();
}
public class StockTransferReceiveItemRequest
{
    public Guid StockTransferItemId { get; set; }
    public decimal ReceivedQuantity { get; set; }
}
