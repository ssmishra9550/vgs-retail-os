using VGS.RetailOS.Modules.SalesManagement.Sale.BO;

namespace VGS.RetailOS.Modules.SalesManagement.Sale.IDAC;

public interface ISaleDAC
{
    Task<SaleBO> CreateDraftSaleAsync(SaleBO sale, CancellationToken cancellationToken);
    Task<SaleBO?> GetSaleByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken);
    Task<IEnumerable<SaleBO>> GetDraftSalesAsync(Guid storeId, string tenantId, CancellationToken cancellationToken);
    Task<IEnumerable<SaleBO>> GetSalesHistoryAsync(Guid storeId, string tenantId, CancellationToken cancellationToken);
    Task<SaleBO> CompleteSaleAsync(Guid id, string tenantId, CancellationToken cancellationToken);
    Task<SaleBO> CancelSaleAsync(Guid id, string tenantId, CancellationToken cancellationToken);
    Task<SaleBO> ReturnSaleAsync(Guid id, string tenantId, CancellationToken cancellationToken);
}
