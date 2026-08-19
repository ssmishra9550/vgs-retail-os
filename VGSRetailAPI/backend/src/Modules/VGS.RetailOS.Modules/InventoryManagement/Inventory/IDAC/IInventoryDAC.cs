using VGS.RetailOS.Modules.InventoryManagement.Inventory.BO;

namespace VGS.RetailOS.Modules.InventoryManagement.Inventory.IDAC;

public interface IInventoryDAC
{
    /// <summary>
    /// Records a stock transaction in the ledger and updates the materialized stock balance in a single database transaction.
    /// </summary>
    Task<InventoryLedgerBO> RecordTransactionAsync(InventoryLedgerBO transaction, CancellationToken cancellationToken);

    Task<StockBalanceBO?> GetStockBalanceAsync(string tenantId, Guid storeId, Guid productId, CancellationToken cancellationToken);
    
    Task<List<StockBalanceBO>> GetAllStockBalancesAsync(string tenantId, Guid storeId, CancellationToken cancellationToken);
    
    Task<List<InventoryLedgerBO>> GetStockHistoryAsync(string tenantId, Guid storeId, Guid productId, CancellationToken cancellationToken);
}
