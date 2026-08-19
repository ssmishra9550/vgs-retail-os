namespace VGS.RetailOS.Modules.InventoryManagement.Inventory.BO;

public class StockBalanceBO
{
    public Guid StoreId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
}
