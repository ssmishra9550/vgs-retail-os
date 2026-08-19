using VGS.RetailOS.Contracts.V1.InventoryManagement.Requests;
using VGS.RetailOS.Contracts.V1.InventoryManagement.Responses;

namespace VGS.RetailOS.Modules.InventoryManagement.Inventory.IBL;

public interface IInventoryBL
{
    Task<InventoryLedgerResponse> RecordTransactionAsync(RecordStockTransactionRequest request, CancellationToken cancellationToken);
    Task<StockBalanceResponse?> GetStockBalanceAsync(Guid storeId, Guid productId, CancellationToken cancellationToken);
    Task<List<StockBalanceResponse>> GetAllStockBalancesAsync(Guid storeId, CancellationToken cancellationToken);
    Task<List<InventoryLedgerResponse>> GetStockHistoryAsync(Guid storeId, Guid productId, CancellationToken cancellationToken);
}
