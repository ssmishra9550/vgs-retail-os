using VGS.RetailOS.Shared.Audit;
using VGS.RetailOS.Infrastructure.Store.DAC.Entities;

namespace VGS.RetailOS.Infrastructure.InventoryManagement.DAC.Entities;

public class StockTransferEntity : IAuditableEntity
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = null!;
    public string TransferNumber { get; set; } = null!;
    public Guid SourceStoreId { get; set; }
    public StoreEntity? SourceStore { get; set; }
    public Guid DestinationStoreId { get; set; }
    public StoreEntity? DestinationStore { get; set; }
    
    public string Status { get; set; } = "Initiated"; // Initiated, InTransit, Received, Cancelled
    public DateTimeOffset? ShippedAt { get; set; }
    public DateTimeOffset? ReceivedAt { get; set; }
    
    public ICollection<StockTransferItemEntity> Items { get; set; } = new List<StockTransferItemEntity>();

    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
