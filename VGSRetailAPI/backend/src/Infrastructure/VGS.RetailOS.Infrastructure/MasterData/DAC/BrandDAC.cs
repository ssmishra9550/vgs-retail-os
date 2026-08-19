using Microsoft.EntityFrameworkCore;
using VGS.RetailOS.Infrastructure.Data;
using VGS.RetailOS.Infrastructure.MasterData.DAC.Entities;
using VGS.RetailOS.Modules.MasterData.Brand.BO;
using VGS.RetailOS.Modules.MasterData.Brand.IDAC;

namespace VGS.RetailOS.Infrastructure.MasterData.DAC;

public class BrandDAC : IBrandDAC
{
    private readonly AppDbContext _dbContext;

    public BrandDAC(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BrandBO?> GetBrandByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Brands.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId, cancellationToken);
            
        return MapToBO(entity);
    }

    public async Task<BrandBO?> GetBrandByNameAsync(string name, string tenantId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Brands.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Name == name && b.TenantId == tenantId, cancellationToken);
            
        return MapToBO(entity);
    }

    public async Task<List<BrandBO>> GetAllBrandsAsync(string tenantId, CancellationToken cancellationToken)
    {
        var entities = await _dbContext.Brands.AsNoTracking()
            .Where(b => b.TenantId == tenantId)
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);
            
        return entities.Select(MapToBO).Where(x => x != null).Cast<BrandBO>().ToList();
    }

    public async Task<BrandBO> CreateBrandAsync(BrandBO brand, CancellationToken cancellationToken)
    {
        var entity = new BrandEntity
        {
            Id = brand.Id,
            TenantId = brand.TenantId,
            Name = brand.Name,
            Description = brand.Description,
            IsActive = brand.IsActive
        };

        _dbContext.Brands.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return brand;
    }

    public async Task<BrandBO> UpdateBrandAsync(BrandBO brand, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Brands
            .FirstOrDefaultAsync(b => b.Id == brand.Id && b.TenantId == brand.TenantId, cancellationToken);

        if (entity != null)
        {
            entity.Name = brand.Name;
            entity.Description = brand.Description;
            entity.IsActive = brand.IsActive;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return brand;
    }

    private BrandBO? MapToBO(BrandEntity? entity)
    {
        if (entity == null) return null;
        
        return new BrandBO
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive
        };
    }
}
