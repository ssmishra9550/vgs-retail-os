using VGS.RetailOS.Contracts.V1.InventoryManagement.Requests;
using VGS.RetailOS.Modules.InventoryManagement.StockTransfer.BO;
using VGS.RetailOS.Modules.InventoryManagement.StockTransfer.IBL;
using VGS.RetailOS.Modules.InventoryManagement.StockTransfer.IDAC;
using VGS.RetailOS.Shared.Tenancy;
using VGS.RetailOS.Shared.Errors.Exceptions;
namespace VGS.RetailOS.Modules.InventoryManagement.StockTransfer.BL;
public class StockTransferBL : IStockTransferBL
{
    private readonly IStockTransferDAC _dac;
    private readonly ITenantContextAccessor _tenantAccessor;
    public StockTransferBL(IStockTransferDAC dac, ITenantContextAccessor tenantAccessor) { _dac = dac; _tenantAccessor = tenantAccessor; }

    public async Task<StockTransferBO> InitiateTransferAsync(InitiateStockTransferRequest request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantAccessor.TenantContext?.CurrentTenantId ?? throw new UnauthorizedException("No tenant");
        var bo = new StockTransferBO {
            Id = Guid.NewGuid(), TenantId = tenantId, TransferNumber = $"TRN-{DateTime.UtcNow:yyyyMMddHHmmss}",
            SourceStoreId = request.SourceStoreId, DestinationStoreId = request.DestinationStoreId,
            Status = "Initiated", ShippedAt = DateTimeOffset.UtcNow
        };
        return await _dac.CreateTransferAsync(bo, cancellationToken);
    }
    public async Task<StockTransferBO> GetTransferAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = _tenantAccessor.TenantContext?.CurrentTenantId ?? throw new UnauthorizedException("No tenant");
        return await _dac.GetTransferByIdAsync(id, tenantId, cancellationToken) ?? throw new NotFoundException("Not found");
    }
    public async Task<List<StockTransferBO>> GetAllTransfersAsync(CancellationToken cancellationToken)
    {
        var tenantId = _tenantAccessor.TenantContext?.CurrentTenantId ?? throw new UnauthorizedException("No tenant");
        return await _dac.GetAllTransfersAsync(tenantId, cancellationToken);
    }
}
