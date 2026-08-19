using VGS.RetailOS.Modules.InventoryManagement.StockTransfer.BO;
namespace VGS.RetailOS.Modules.InventoryManagement.StockTransfer.IDAC;
public interface IStockTransferDAC
{
    Task<StockTransferBO> CreateTransferAsync(StockTransferBO transfer, CancellationToken cancellationToken);
    Task<StockTransferBO?> GetTransferByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken);
    Task<List<StockTransferBO>> GetAllTransfersAsync(string tenantId, CancellationToken cancellationToken);
}
