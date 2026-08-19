using VGS.RetailOS.Contracts.V1.ReturnsManagement.Requests;
using VGS.RetailOS.Modules.ReturnsManagement.Return.BO;
using VGS.RetailOS.Modules.ReturnsManagement.Return.IBL;
using VGS.RetailOS.Modules.ReturnsManagement.Return.IDAC;
using VGS.RetailOS.Shared.Tenancy;
using VGS.RetailOS.Shared.Errors.Exceptions;
namespace VGS.RetailOS.Modules.ReturnsManagement.Return.BL;
public class ReturnBL : IReturnBL
{
    private readonly IReturnDAC _dac;
    private readonly ITenantContextAccessor _tenantAccessor;
    public ReturnBL(IReturnDAC dac, ITenantContextAccessor tenantAccessor) { _dac = dac; _tenantAccessor = tenantAccessor; }

    public async Task<ReturnBO> ProcessReturnAsync(CreateReturnRequest request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantAccessor.TenantContext?.CurrentTenantId ?? throw new UnauthorizedException("No tenant");
        var bo = new ReturnBO {
            Id = Guid.NewGuid(), TenantId = tenantId, ReturnNumber = $"RET-{DateTime.UtcNow:yyyyMMddHHmmss}",
            ReturnType = request.ReturnType, StoreId = request.StoreId, TotalAmount = request.TotalAmount
        };
        return await _dac.CreateReturnAsync(bo, cancellationToken);
    }
    public async Task<List<ReturnBO>> GetAllReturnsAsync(CancellationToken cancellationToken)
    {
        var tenantId = _tenantAccessor.TenantContext?.CurrentTenantId ?? throw new UnauthorizedException("No tenant");
        return await _dac.GetAllReturnsAsync(tenantId, cancellationToken);
    }
}
