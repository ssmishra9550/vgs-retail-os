using VGS.RetailOS.Contracts.V1.SalesManagement.Requests;
using VGS.RetailOS.Contracts.V1.SalesManagement.Responses;

namespace VGS.RetailOS.Modules.SalesManagement.Sale.IBL;

public interface ISaleBL
{
    Task<SaleResponse> CreateDraftSaleAsync(CreateSaleRequest request, CancellationToken cancellationToken);
    Task<SaleResponse?> GetSaleByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<SaleResponse> CompleteSaleAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<SaleResponse>> GetDraftSalesAsync(Guid storeId, CancellationToken cancellationToken);
    Task<IEnumerable<SaleResponse>> GetSalesHistoryAsync(Guid storeId, CancellationToken cancellationToken);
    Task<SaleResponse> CancelSaleAsync(Guid id, CancellationToken cancellationToken);
    Task<SaleResponse> ProcessReturnAsync(Guid id, ProcessReturnRequest request, CancellationToken cancellationToken);
}
