using VGS.RetailOS.Contracts.V1.PurchasingManagement.Requests;
using VGS.RetailOS.Contracts.V1.PurchasingManagement.Responses;

namespace VGS.RetailOS.Modules.PurchasingManagement.Purchase.IBL;

public interface IPurchaseBL
{
    Task<PurchaseResponse> CreateDraftPurchaseAsync(CreatePurchaseRequest request, CancellationToken cancellationToken);
    Task<PurchaseResponse?> GetPurchaseByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<PurchaseResponse>> GetAllPurchasesAsync(Guid storeId, CancellationToken cancellationToken);
    Task<PurchaseResponse> ReceivePurchaseAsync(Guid purchaseId, CancellationToken cancellationToken);
}
