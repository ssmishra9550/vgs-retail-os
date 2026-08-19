namespace VGS.RetailOS.Infrastructure.InventoryManagement.DAC.Entities;

public class StockBalanceEntity
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = null!;
    public Guid StoreId { get; set; }
    public Guid ProductId { get; set; }
    
    public decimal Quantity { get; set; }
    
    public DateTimeOffset LastUpdated { get; set; }

    // Concurrency token to prevent race conditions during concurrent stock updates
    public uint Version { get; set; }
}
