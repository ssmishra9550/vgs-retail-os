using VGS.RetailOS.Contracts.V1.MasterData.Requests;
using VGS.RetailOS.Contracts.V1.MasterData.Responses;
using VGS.RetailOS.Modules.MasterData.Tax.BO;
using VGS.RetailOS.Modules.MasterData.Tax.IBL;
using VGS.RetailOS.Modules.MasterData.Tax.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;

namespace VGS.RetailOS.Modules.MasterData.Tax.BL;

public class TaxBL : ITaxBL
{
    private readonly ITaxDAC _taxDac;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public TaxBL(ITaxDAC taxDac, ITenantContextAccessor tenantContextAccessor)
    {
        _taxDac = taxDac;
        _tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<TaxResponse> GetTaxByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var tax = await _taxDac.GetTaxByIdAsync(id, tenantId, cancellationToken);
        
        if (tax == null)
            throw new NotFoundException($"Tax with ID {id} not found.");

        return MapToResponse(tax);
    }

    public async Task<List<TaxResponse>> GetAllTaxesAsync(CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var taxes = await _taxDac.GetAllTaxesAsync(tenantId, cancellationToken);
        
        return taxes.Select(MapToResponse).ToList();
    }

    public async Task<TaxResponse> CreateTaxAsync(CreateTaxRequest request, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();

        var existing = await _taxDac.GetTaxByNameAsync(request.Name, tenantId, cancellationToken);
        if (existing != null)
            throw new ValidationException($"Tax with name '{request.Name}' already exists.");

        if (request.Type != "Percentage" && request.Type != "FixedAmount")
            throw new ValidationException("Tax type must be either 'Percentage' or 'FixedAmount'.");

        var taxBo = new TaxBO
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Rate = request.Rate,
            Type = request.Type,
            IsActive = true
        };

        var created = await _taxDac.CreateTaxAsync(taxBo, cancellationToken);
        return MapToResponse(created);
    }

    public async Task<TaxResponse> UpdateTaxAsync(Guid id, UpdateTaxRequest request, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();

        var tax = await _taxDac.GetTaxByIdAsync(id, tenantId, cancellationToken);
        if (tax == null)
            throw new NotFoundException($"Tax with ID {id} not found.");

        if (!tax.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _taxDac.GetTaxByNameAsync(request.Name, tenantId, cancellationToken);
            if (existing != null && existing.Id != id)
                throw new ValidationException($"Tax with name '{request.Name}' already exists.");
        }

        if (request.Type != "Percentage" && request.Type != "FixedAmount")
            throw new ValidationException("Tax type must be either 'Percentage' or 'FixedAmount'.");

        tax.Name = request.Name;
        tax.Rate = request.Rate;
        tax.Type = request.Type;
        tax.IsActive = request.IsActive;

        var updated = await _taxDac.UpdateTaxAsync(tax, cancellationToken);
        return MapToResponse(updated);
    }

    private string GetTenantId()
    {
        var tenantId = _tenantContextAccessor.TenantContext?.CurrentTenantId;
        if (string.IsNullOrEmpty(tenantId))
            throw new UnauthorizedException("Tenant context is missing.");
        return tenantId;
    }

    private TaxResponse MapToResponse(TaxBO bo)
    {
        return new TaxResponse
        {
            Id = bo.Id,
            Name = bo.Name,
            Rate = bo.Rate,
            Type = bo.Type,
            IsActive = bo.IsActive
        };
    }
}
