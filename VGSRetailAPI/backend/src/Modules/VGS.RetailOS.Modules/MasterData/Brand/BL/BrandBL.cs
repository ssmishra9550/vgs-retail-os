using VGS.RetailOS.Contracts.V1.MasterData.Requests;
using VGS.RetailOS.Contracts.V1.MasterData.Responses;
using VGS.RetailOS.Modules.MasterData.Brand.BO;
using VGS.RetailOS.Modules.MasterData.Brand.IBL;
using VGS.RetailOS.Modules.MasterData.Brand.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;

namespace VGS.RetailOS.Modules.MasterData.Brand.BL;

public class BrandBL : IBrandBL
{
    private readonly IBrandDAC _brandDac;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public BrandBL(IBrandDAC brandDac, ITenantContextAccessor tenantContextAccessor)
    {
        _brandDac = brandDac;
        _tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<BrandResponse> GetBrandByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var brand = await _brandDac.GetBrandByIdAsync(id, tenantId, cancellationToken);
        
        if (brand == null)
            throw new NotFoundException($"Brand with ID {id} not found.");

        return MapToResponse(brand);
    }

    public async Task<List<BrandResponse>> GetAllBrandsAsync(CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var brands = await _brandDac.GetAllBrandsAsync(tenantId, cancellationToken);
        
        return brands.Select(MapToResponse).ToList();
    }

    public async Task<BrandResponse> CreateBrandAsync(CreateBrandRequest request, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();

        var existing = await _brandDac.GetBrandByNameAsync(request.Name, tenantId, cancellationToken);
        if (existing != null)
            throw new ValidationException($"Brand with name '{request.Name}' already exists.");

        var brandBo = new BrandBO
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Description = request.Description,
            IsActive = true
        };

        var created = await _brandDac.CreateBrandAsync(brandBo, cancellationToken);
        return MapToResponse(created);
    }

    public async Task<BrandResponse> UpdateBrandAsync(Guid id, UpdateBrandRequest request, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();

        var brand = await _brandDac.GetBrandByIdAsync(id, tenantId, cancellationToken);
        if (brand == null)
            throw new NotFoundException($"Brand with ID {id} not found.");

        if (!brand.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _brandDac.GetBrandByNameAsync(request.Name, tenantId, cancellationToken);
            if (existing != null && existing.Id != id)
                throw new ValidationException($"Brand with name '{request.Name}' already exists.");
        }

        brand.Name = request.Name;
        brand.Description = request.Description;
        brand.IsActive = request.IsActive;

        var updated = await _brandDac.UpdateBrandAsync(brand, cancellationToken);
        return MapToResponse(updated);
    }

    private string GetTenantId()
    {
        var tenantId = _tenantContextAccessor.TenantContext?.CurrentTenantId;
        if (string.IsNullOrEmpty(tenantId))
            throw new UnauthorizedException("Tenant context is missing.");
        return tenantId;
    }

    private BrandResponse MapToResponse(BrandBO bo)
    {
        return new BrandResponse
        {
            Id = bo.Id,
            Name = bo.Name,
            Description = bo.Description,
            IsActive = bo.IsActive
        };
    }
}
