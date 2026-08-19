using Microsoft.EntityFrameworkCore;
using VGS.RetailOS.Infrastructure.Data;
using VGS.RetailOS.Infrastructure.Store.DAC.Mapping;
using VGS.RetailOS.Modules.Store.BO;
using VGS.RetailOS.Modules.Store.IDAC;

namespace VGS.RetailOS.Infrastructure.Store.DAC;

public class StoreDAC : IStoreDAC
{
    private readonly AppDbContext _dbContext;

    public StoreDAC(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<StoreBO> CreateAsync(StoreBO store, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(store);

        var entity = store.ToEntity();
        
        _dbContext.Stores.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.ToStoreBO();
    }

    public async Task<StoreBO> UpdateAsync(StoreBO store, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(store);

        var entity = store.ToEntity();
        
        _dbContext.Stores.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.ToStoreBO();
    }

    public async Task<StoreBO?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Stores
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        return entity?.ToStoreBO();
    }

    public async Task<List<StoreBO>> GetByOrganizationIdAsync(Guid organizationId, string tenantId, CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.Stores
            .AsNoTracking()
            .Where(s => s.OrganizationId == organizationId && s.TenantId == tenantId)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        return entities.Select(e => e.ToStoreBO()).ToList();
    }

    public async Task<List<StoreBO>> GetAllForTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.Stores
            .AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        return entities.Select(e => e.ToStoreBO()).ToList();
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid organizationId, string tenantId, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var query = _dbContext.Stores
            .AsNoTracking()
            .Where(s => s.Name == name && s.OrganizationId == organizationId && s.TenantId == tenantId);

        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string code, Guid organizationId, string tenantId, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        var query = _dbContext.Stores
            .AsNoTracking()
            .Where(s => s.Code == code && s.OrganizationId == organizationId && s.TenantId == tenantId);

        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
