using Microsoft.EntityFrameworkCore;
using VGS.RetailOS.Infrastructure.Data;
using VGS.RetailOS.Infrastructure.Settings.DAC.Entities;
using VGS.RetailOS.Modules.Settings.Setting.BO;
using VGS.RetailOS.Modules.Settings.Setting.IDAC;

namespace VGS.RetailOS.Infrastructure.Settings.DAC;

public class SettingDAC : ISettingDAC
{
    private readonly AppDbContext _dbContext;

    public SettingDAC(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SettingBO?> GetSettingAsync(string key, string tenantId, Guid? storeId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Settings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key == key && s.TenantId == tenantId && s.StoreId == storeId, cancellationToken);

        return entity == null ? null : MapToBO(entity);
    }

    public async Task<List<SettingBO>> GetAllSettingsAsync(string tenantId, Guid? storeId, CancellationToken cancellationToken)
    {
        var query = _dbContext.Settings.AsNoTracking().Where(s => s.TenantId == tenantId);

        if (storeId.HasValue)
        {
            // We want to return both tenant-level settings (StoreId == null) and store-specific settings
            query = query.Where(s => s.StoreId == null || s.StoreId == storeId.Value);
        }
        else
        {
            // Only return global tenant settings
            query = query.Where(s => s.StoreId == null);
        }

        var entities = await query.ToListAsync(cancellationToken);
        return entities.Select(MapToBO).ToList();
    }

    public async Task<SettingBO> UpsertSettingAsync(SettingBO setting, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Settings
            .FirstOrDefaultAsync(s => s.Key == setting.Key && s.TenantId == setting.TenantId && s.StoreId == setting.StoreId, cancellationToken);

        if (entity != null)
        {
            entity.Value = setting.Value;
            entity.Group = setting.Group;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return MapToBO(entity);
        }

        entity = new SettingEntity
        {
            Id = Guid.NewGuid(),
            TenantId = setting.TenantId,
            StoreId = setting.StoreId,
            Key = setting.Key,
            Value = setting.Value,
            Group = setting.Group
        };

        _dbContext.Settings.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToBO(entity);
    }

    private static SettingBO MapToBO(SettingEntity entity)
    {
        return new SettingBO
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            StoreId = entity.StoreId,
            Key = entity.Key,
            Value = entity.Value,
            Group = entity.Group
        };
    }
}
