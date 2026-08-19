namespace VGS.RetailOS.Modules.InventoryManagement.StockTransfer.BO;
public class StockTransferBO
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = null!;
    public string TransferNumber { get; set; } = null!;
    public Guid SourceStoreId { get; set; }
    public Guid DestinationStoreId { get; set; }
    public string Status { get; set; } = "Initiated";
    public DateTimeOffset? ShippedAt { get; set; }
    public DateTimeOffset? ReceivedAt { get; set; }
}
