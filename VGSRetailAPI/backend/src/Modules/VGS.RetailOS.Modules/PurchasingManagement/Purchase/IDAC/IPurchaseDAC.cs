using VGS.RetailOS.Modules.PurchasingManagement.Purchase.BO;

namespace VGS.RetailOS.Modules.PurchasingManagement.Purchase.IDAC;

public interface IPurchaseDAC
{
    Task<PurchaseBO> CreateDraftPurchaseAsync(PurchaseBO purchase, CancellationToken cancellationToken);
    Task<PurchaseBO?> GetPurchaseByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken);
    Task<IEnumerable<PurchaseBO>> GetAllPurchasesAsync(Guid storeId, string tenantId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Receives a purchase. This assumes that inventory balances and supplier payables
    /// will be updated within the same transaction scope outside of this method, or orchestrated together.
    /// This method only changes the Purchase Status to Received.
    /// </summary>
    Task<PurchaseBO> MarkAsReceivedAsync(Guid purchaseId, string tenantId, CancellationToken cancellationToken);
}
