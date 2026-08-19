using VGS.RetailOS.Contracts.V1.InventoryManagement.Requests;
using VGS.RetailOS.Contracts.V1.InventoryManagement.Responses;
using VGS.RetailOS.Modules.InventoryManagement.Inventory.BO;
using VGS.RetailOS.Modules.InventoryManagement.Inventory.IBL;
using VGS.RetailOS.Modules.InventoryManagement.Inventory.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;

namespace VGS.RetailOS.Modules.InventoryManagement.Inventory.BL;

public class InventoryBL : IInventoryBL
{
    private readonly IInventoryDAC _inventoryDac;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public InventoryBL(IInventoryDAC inventoryDac, ITenantContextAccessor tenantContextAccessor)
    {
        _inventoryDac = inventoryDac;
        _tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<InventoryLedgerResponse> RecordTransactionAsync(RecordStockTransactionRequest request, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();

        if (request.ChangeQuantity == 0)
        {
            throw new ValidationException("Change quantity cannot be zero.");
        }

        // We could implement negative stock blocking here if strict policy is enabled,
        // but for MVP we allow it and just log it in the ledger.

        var transactionBo = new InventoryLedgerBO
        {
            TenantId = tenantId,
            StoreId = request.StoreId,
            ProductId = request.ProductId,
            ChangeQuantity = request.ChangeQuantity,
            TransactionType = request.TransactionType,
            ReferenceId = request.ReferenceId,
            Reason = request.Reason
        };

        var recordedBo = await _inventoryDac.RecordTransactionAsync(transactionBo, cancellationToken);

        return new InventoryLedgerResponse
        {
            Id = recordedBo.Id,
            StoreId = recordedBo.StoreId,
            ProductId = recordedBo.ProductId,
            ChangeQuantity = recordedBo.ChangeQuantity,
            BalanceAfter = recordedBo.BalanceAfter,
            TransactionType = recordedBo.TransactionType,
            ReferenceId = recordedBo.ReferenceId,
            Reason = recordedBo.Reason,
            CreatedAt = recordedBo.CreatedAt
        };
    }

    public async Task<StockBalanceResponse?> GetStockBalanceAsync(Guid storeId, Guid productId, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var balanceBo = await _inventoryDac.GetStockBalanceAsync(tenantId, storeId, productId, cancellationToken);

        if (balanceBo == null) return null;

        return new StockBalanceResponse
        {
            StoreId = balanceBo.StoreId,
            ProductId = balanceBo.ProductId,
            Quantity = balanceBo.Quantity,
            LastUpdated = balanceBo.LastUpdated
        };
    }

    public async Task<List<StockBalanceResponse>> GetAllStockBalancesAsync(Guid storeId, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var balances = await _inventoryDac.GetAllStockBalancesAsync(tenantId, storeId, cancellationToken);

        return balances.Select(b => new StockBalanceResponse
        {
            StoreId = b.StoreId,
            ProductId = b.ProductId,
            Quantity = b.Quantity,
            LastUpdated = b.LastUpdated
        }).ToList();
    }

    public async Task<List<InventoryLedgerResponse>> GetStockHistoryAsync(Guid storeId, Guid productId, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var history = await _inventoryDac.GetStockHistoryAsync(tenantId, storeId, productId, cancellationToken);

        return history.Select(h => new InventoryLedgerResponse
        {
            Id = h.Id,
            StoreId = h.StoreId,
            ProductId = h.ProductId,
            ChangeQuantity = h.ChangeQuantity,
            BalanceAfter = h.BalanceAfter,
            TransactionType = h.TransactionType,
            ReferenceId = h.ReferenceId,
            Reason = h.Reason,
            CreatedAt = h.CreatedAt
        }).ToList();
    }

    private string GetTenantId()
    {
        var tenantId = _tenantContextAccessor.TenantContext?.CurrentTenantId;
        if (string.IsNullOrEmpty(tenantId))
            throw new UnauthorizedException("Tenant context is missing.");
        return tenantId;
    }
}
