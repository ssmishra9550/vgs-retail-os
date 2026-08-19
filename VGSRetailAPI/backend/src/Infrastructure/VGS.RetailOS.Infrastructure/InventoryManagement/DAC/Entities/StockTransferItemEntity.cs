using VGS.RetailOS.Infrastructure.ProductManagement.DAC.Entities;
namespace VGS.RetailOS.Infrastructure.InventoryManagement.DAC.Entities;

public class StockTransferItemEntity
{
    public Guid Id { get; set; }
    public Guid StockTransferId { get; set; }
    public StockTransferEntity? StockTransfer { get; set; }
    
    public Guid ProductId { get; set; }
    public ProductEntity? Product { get; set; }
    
    public decimal Quantity { get; set; }
    public decimal? ReceivedQuantity { get; set; }
}
