using VGS.RetailOS.Contracts.V1.InventoryManagement.Requests;
using VGS.RetailOS.Modules.InventoryManagement.StockTransfer.BO;
namespace VGS.RetailOS.Modules.InventoryManagement.StockTransfer.IBL;
public interface IStockTransferBL
{
    Task<StockTransferBO> InitiateTransferAsync(InitiateStockTransferRequest request, CancellationToken cancellationToken);
    Task<StockTransferBO> GetTransferAsync(Guid id, CancellationToken cancellationToken);
    Task<List<StockTransferBO>> GetAllTransfersAsync(CancellationToken cancellationToken);
}
