using VGS.RetailOS.Contracts.V1.MasterData.Requests;
using VGS.RetailOS.Contracts.V1.MasterData.Responses;
using VGS.RetailOS.Modules.MasterData.Unit.BO;
using VGS.RetailOS.Modules.MasterData.Unit.IBL;
using VGS.RetailOS.Modules.MasterData.Unit.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;

namespace VGS.RetailOS.Modules.MasterData.Unit.BL;

public class UnitBL : IUnitBL
{
    private readonly IUnitDAC _unitDac;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public UnitBL(IUnitDAC unitDac, ITenantContextAccessor tenantContextAccessor)
    {
        _unitDac = unitDac;
        _tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<UnitResponse> GetUnitByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var unit = await _unitDac.GetUnitByIdAsync(id, tenantId, cancellationToken);
        
        if (unit == null)
            throw new NotFoundException($"Unit with ID {id} not found.");

        return MapToResponse(unit);
    }

    public async Task<List<UnitResponse>> GetAllUnitsAsync(CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var units = await _unitDac.GetAllUnitsAsync(tenantId, cancellationToken);
        
        return units.Select(MapToResponse).ToList();
    }

    public async Task<UnitResponse> CreateUnitAsync(CreateUnitRequest request, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();

        var existing = await _unitDac.GetUnitByNameAsync(request.Name, tenantId, cancellationToken);
        if (existing != null)
            throw new ValidationException($"Unit with name '{request.Name}' already exists.");

        var unitBo = new UnitBO
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            ShortName = request.ShortName,
            IsActive = true
        };

        var created = await _unitDac.CreateUnitAsync(unitBo, cancellationToken);
        return MapToResponse(created);
    }

    public async Task<UnitResponse> UpdateUnitAsync(Guid id, UpdateUnitRequest request, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();

        var unit = await _unitDac.GetUnitByIdAsync(id, tenantId, cancellationToken);
        if (unit == null)
            throw new NotFoundException($"Unit with ID {id} not found.");

        if (!unit.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _unitDac.GetUnitByNameAsync(request.Name, tenantId, cancellationToken);
            if (existing != null && existing.Id != id)
                throw new ValidationException($"Unit with name '{request.Name}' already exists.");
        }

        unit.Name = request.Name;
        unit.ShortName = request.ShortName;
        unit.IsActive = request.IsActive;

        var updated = await _unitDac.UpdateUnitAsync(unit, cancellationToken);
        return MapToResponse(updated);
    }

    private string GetTenantId()
    {
        var tenantId = _tenantContextAccessor.TenantContext?.CurrentTenantId;
        if (string.IsNullOrEmpty(tenantId))
            throw new UnauthorizedException("Tenant context is missing.");
        return tenantId;
    }

    private UnitResponse MapToResponse(UnitBO bo)
    {
        return new UnitResponse
        {
            Id = bo.Id,
            Name = bo.Name,
            ShortName = bo.ShortName,
            IsActive = bo.IsActive
        };
    }
}
