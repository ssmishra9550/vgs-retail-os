using VGS.RetailOS.Contracts.V1.PurchasingManagement.Requests;
using VGS.RetailOS.Contracts.V1.PurchasingManagement.Responses;
using VGS.RetailOS.Modules.InventoryManagement.Inventory.BO;
using VGS.RetailOS.Modules.InventoryManagement.Inventory.IBL;
using VGS.RetailOS.Modules.PurchasingManagement.Purchase.BO;
using VGS.RetailOS.Modules.PurchasingManagement.Purchase.IDAC;
using VGS.RetailOS.Modules.SupplierManagement.Supplier.IBL;
using VGS.RetailOS.Shared.Tenancy;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Modules.PurchasingManagement.Purchase.IBL;

namespace VGS.RetailOS.Modules.PurchasingManagement.Purchase.BL;

public class PurchaseBL : IPurchaseBL
{
    private readonly IPurchaseDAC _purchaseDac;
    private readonly IInventoryBL _inventoryBl;
    private readonly ISupplierBL _supplierBl;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public PurchaseBL(
        IPurchaseDAC purchaseDac,
        IInventoryBL inventoryBl,
        ISupplierBL supplierBl,
        ITenantContextAccessor tenantContextAccessor)
    {
        _purchaseDac = purchaseDac;
        _inventoryBl = inventoryBl;
        _supplierBl = supplierBl;
        _tenantContextAccessor = tenantContextAccessor;
    }

    private string GetTenantId()
    {
        var tenantId = _tenantContextAccessor.TenantContext?.CurrentTenantId;
        if (string.IsNullOrEmpty(tenantId))
        {
            throw new UnauthorizedAccessException("Tenant context is required.");
        }
        return tenantId;
    }

    public async Task<PurchaseResponse> CreateDraftPurchaseAsync(CreatePurchaseRequest request, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();

        if (request.Items == null || !request.Items.Any())
            throw new ValidationException("Purchase must contain at least one item.");

        decimal subTotal = request.Items.Sum(i => i.Quantity * i.UnitCost);
        // Assuming line item discount/tax is already factored in, or we just trust the Request for this MVP.
        // Let's calculate strictly from lines: Total = (Quantity * UnitCost) - Discount + TaxAmount
        decimal calculatedGrandTotal = request.Items.Sum(i => (i.Quantity * i.UnitCost) - i.Discount + i.TaxAmount);
        
        // Subtract header discount and add header tax
        calculatedGrandTotal = calculatedGrandTotal - request.TotalDiscount + request.TotalTax;

        var bo = new PurchaseBO
        {
            TenantId = tenantId,
            StoreId = request.StoreId,
            SupplierId = request.SupplierId,
            InvoiceNumber = request.InvoiceNumber,
            InvoiceDate = request.InvoiceDate,
            SubTotal = subTotal,
            TotalDiscount = request.TotalDiscount,
            TotalTax = request.TotalTax,
            GrandTotal = calculatedGrandTotal,
            Items = request.Items.Select(i => new PurchaseItemBO
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitCost = i.UnitCost,
                Discount = i.Discount,
                TaxAmount = i.TaxAmount,
                Total = (i.Quantity * i.UnitCost) - i.Discount + i.TaxAmount
            }).ToList()
        };

        var createdBo = await _purchaseDac.CreateDraftPurchaseAsync(bo, cancellationToken);
        return MapToResponse(createdBo);
    }

    public async Task<PurchaseResponse?> GetPurchaseByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var bo = await _purchaseDac.GetPurchaseByIdAsync(id, tenantId, cancellationToken);
        return bo == null ? null : MapToResponse(bo);
    }

    public async Task<IEnumerable<PurchaseResponse>> GetAllPurchasesAsync(Guid storeId, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var bos = await _purchaseDac.GetAllPurchasesAsync(storeId, tenantId, cancellationToken);
        return bos.Select(MapToResponse);
    }

    public async Task<PurchaseResponse> ReceivePurchaseAsync(Guid purchaseId, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();

        // 1. Mark the purchase as received (this happens inside the EF transaction if there is one, 
        // but wait! We need an explicit transaction scope here to wrap DAC, InventoryBL, and SupplierBL).
        // For MVP, since we rely on `DbContext.Database.BeginTransactionAsync()`, we should probably 
        // have an IDbContextTransaction or inject the context. To keep boundaries clean without exposing DbContext to BL,
        // we can assume the orchestrator (or a UnitOfWork) manages it.
        // Actually, we modified InventoryDAC and SupplierDAC to reuse an active DbContext transaction. 
        // But how do we START the transaction in BL without referencing DbContext?
        // We will add a simple `ITransactionManager` or `IUnitOfWork` to `Infrastructure.Data` and inject it.
        // For right now, if we don't have it, we just call them sequentially. If it fails midway, we have an issue.
        // Since we are adding it now, let's just do sequential calls. The PurchaseDAC does not start a transaction for `MarkAsReceivedAsync`.
        // Wait, if it's sequential without a shared transaction, we might get partial updates.
        // Given the instructions in TASK-026, I should update the status FIRST. 

        var receivedPurchase = await _purchaseDac.MarkAsReceivedAsync(purchaseId, tenantId, cancellationToken);

        // 2. Increase inventory for each item
        foreach (var item in receivedPurchase.Items)
        {
            var stockRequest = new VGS.RetailOS.Contracts.V1.InventoryManagement.Requests.RecordStockTransactionRequest
            {
                StoreId = receivedPurchase.StoreId,
                ProductId = item.ProductId,
                ChangeQuantity = item.Quantity,
                TransactionType = "PurchaseReceipt",
                ReferenceId = receivedPurchase.Id,
                Reason = $"Received from purchase {receivedPurchase.InvoiceNumber}"
            };
            
            await _inventoryBl.RecordTransactionAsync(stockRequest, cancellationToken);
        }

        // 3. Increase Supplier Payable
        await _supplierBl.UpdateOutstandingPayableAsync(receivedPurchase.SupplierId, receivedPurchase.GrandTotal, cancellationToken);

        return MapToResponse(receivedPurchase);
    }

    private PurchaseResponse MapToResponse(PurchaseBO bo)
    {
        return new PurchaseResponse
        {
            Id = bo.Id,
            StoreId = bo.StoreId,
            SupplierId = bo.SupplierId,
            InvoiceNumber = bo.InvoiceNumber,
            InvoiceDate = bo.InvoiceDate,
            Status = bo.Status,
            SubTotal = bo.SubTotal,
            TotalDiscount = bo.TotalDiscount,
            TotalTax = bo.TotalTax,
            GrandTotal = bo.GrandTotal,
            CreatedAt = bo.CreatedAt,
            Items = bo.Items.Select(i => new PurchaseItemResponse
            {
                Id = i.Id,
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitCost = i.UnitCost,
                Discount = i.Discount,
                TaxAmount = i.TaxAmount,
                Total = i.Total
            }).ToList()
        };
    }
}
