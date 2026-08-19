namespace VGS.RetailOS.Contracts.V1.InventoryManagement.Responses;

public class InventoryLedgerResponse
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public Guid ProductId { get; set; }
    public decimal ChangeQuantity { get; set; }
    public decimal BalanceAfter { get; set; }
    public string TransactionType { get; set; } = null!;
    public Guid ReferenceId { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
