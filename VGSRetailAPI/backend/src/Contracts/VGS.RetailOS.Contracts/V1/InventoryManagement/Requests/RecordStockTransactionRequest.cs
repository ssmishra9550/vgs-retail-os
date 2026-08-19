using System.ComponentModel.DataAnnotations;

namespace VGS.RetailOS.Contracts.V1.InventoryManagement.Requests;

public class RecordStockTransactionRequest
{
    [Required]
    public Guid StoreId { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public decimal ChangeQuantity { get; set; }

    [Required]
    [MaxLength(50)]
    public string TransactionType { get; set; } = null!; // E.g., Purchase, Sale, Adjustment

    [Required]
    public Guid ReferenceId { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }
}
