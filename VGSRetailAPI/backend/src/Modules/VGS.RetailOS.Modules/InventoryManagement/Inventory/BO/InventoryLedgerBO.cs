namespace VGS.RetailOS.Modules.InventoryManagement.Inventory.BO;

public class InventoryLedgerBO
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = null!;
    public Guid StoreId { get; set; }
    public Guid ProductId { get; set; }
    public decimal ChangeQuantity { get; set; }
    public decimal BalanceAfter { get; set; }
    public string TransactionType { get; set; } = null!;
    public Guid ReferenceId { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
