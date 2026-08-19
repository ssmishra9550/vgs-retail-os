using VGS.RetailOS.Contracts.V1.MasterData.Requests;
using VGS.RetailOS.Contracts.V1.MasterData.Responses;
using VGS.RetailOS.Modules.MasterData.Category.BO;
using VGS.RetailOS.Modules.MasterData.Category.IBL;
using VGS.RetailOS.Modules.MasterData.Category.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;

namespace VGS.RetailOS.Modules.MasterData.Category.BL;

public class CategoryBL : ICategoryBL
{
    private readonly ICategoryDAC _categoryDac;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public CategoryBL(ICategoryDAC categoryDac, ITenantContextAccessor tenantContextAccessor)
    {
        _categoryDac = categoryDac;
        _tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<CategoryResponse> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var category = await _categoryDac.GetCategoryByIdAsync(id, tenantId, cancellationToken);
        
        if (category == null)
            throw new NotFoundException($"Category with ID {id} not found.");

        return MapToResponse(category);
    }

    public async Task<List<CategoryResponse>> GetAllCategoriesAsync(CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var categories = await _categoryDac.GetAllCategoriesAsync(tenantId, cancellationToken);
        
        return categories.Select(MapToResponse).ToList();
    }

    public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();

        var existing = await _categoryDac.GetCategoryByNameAsync(request.Name, tenantId, cancellationToken);
        if (existing != null)
            throw new ValidationException($"Category with name '{request.Name}' already exists.");

        var categoryBo = new CategoryBO
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Description = request.Description,
            IsActive = true
        };

        var created = await _categoryDac.CreateCategoryAsync(categoryBo, cancellationToken);
        return MapToResponse(created);
    }

    public async Task<CategoryResponse> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();

        var category = await _categoryDac.GetCategoryByIdAsync(id, tenantId, cancellationToken);
        if (category == null)
            throw new NotFoundException($"Category with ID {id} not found.");

        if (!category.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _categoryDac.GetCategoryByNameAsync(request.Name, tenantId, cancellationToken);
            if (existing != null && existing.Id != id)
                throw new ValidationException($"Category with name '{request.Name}' already exists.");
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.IsActive = request.IsActive;

        var updated = await _categoryDac.UpdateCategoryAsync(category, cancellationToken);
        return MapToResponse(updated);
    }

    private string GetTenantId()
    {
        var tenantId = _tenantContextAccessor.TenantContext?.CurrentTenantId;
        if (string.IsNullOrEmpty(tenantId))
            throw new UnauthorizedException("Tenant context is missing.");
        return tenantId;
    }

    private CategoryResponse MapToResponse(CategoryBO bo)
    {
        return new CategoryResponse
        {
            Id = bo.Id,
            Name = bo.Name,
            Description = bo.Description,
            IsActive = bo.IsActive
        };
    }
}
