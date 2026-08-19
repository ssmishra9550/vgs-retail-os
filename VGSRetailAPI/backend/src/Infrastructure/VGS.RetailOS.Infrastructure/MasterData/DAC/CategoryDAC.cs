using Microsoft.EntityFrameworkCore;
using VGS.RetailOS.Infrastructure.Data;
using VGS.RetailOS.Infrastructure.MasterData.DAC.Entities;
using VGS.RetailOS.Modules.MasterData.Category.BO;
using VGS.RetailOS.Modules.MasterData.Category.IDAC;

namespace VGS.RetailOS.Infrastructure.MasterData.DAC;

public class CategoryDAC : ICategoryDAC
{
    private readonly AppDbContext _dbContext;

    public CategoryDAC(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CategoryBO?> GetCategoryByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Categories.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId, cancellationToken);
            
        return MapToBO(entity);
    }

    public async Task<CategoryBO?> GetCategoryByNameAsync(string name, string tenantId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Categories.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name == name && c.TenantId == tenantId, cancellationToken);
            
        return MapToBO(entity);
    }

    public async Task<List<CategoryBO>> GetAllCategoriesAsync(string tenantId, CancellationToken cancellationToken)
    {
        var entities = await _dbContext.Categories.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
            
        return entities.Select(MapToBO).Where(x => x != null).Cast<CategoryBO>().ToList();
    }

    public async Task<CategoryBO> CreateCategoryAsync(CategoryBO category, CancellationToken cancellationToken)
    {
        var entity = new CategoryEntity
        {
            Id = category.Id,
            TenantId = category.TenantId,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive
        };

        _dbContext.Categories.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return category;
    }

    public async Task<CategoryBO> UpdateCategoryAsync(CategoryBO category, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == category.Id && c.TenantId == category.TenantId, cancellationToken);

        if (entity != null)
        {
            entity.Name = category.Name;
            entity.Description = category.Description;
            entity.IsActive = category.IsActive;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return category;
    }

    private CategoryBO? MapToBO(CategoryEntity? entity)
    {
        if (entity == null) return null;
        
        return new CategoryBO
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive
        };
    }
}
