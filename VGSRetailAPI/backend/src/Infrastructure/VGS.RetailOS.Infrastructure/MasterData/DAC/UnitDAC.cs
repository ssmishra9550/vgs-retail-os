using Microsoft.EntityFrameworkCore;
using VGS.RetailOS.Infrastructure.Data;
using VGS.RetailOS.Infrastructure.MasterData.DAC.Entities;
using VGS.RetailOS.Modules.MasterData.Unit.BO;
using VGS.RetailOS.Modules.MasterData.Unit.IDAC;

namespace VGS.RetailOS.Infrastructure.MasterData.DAC;

public class UnitDAC : IUnitDAC
{
    private readonly AppDbContext _dbContext;

    public UnitDAC(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UnitBO?> GetUnitByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Units.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId, cancellationToken);
            
        return MapToBO(entity);
    }

    public async Task<UnitBO?> GetUnitByNameAsync(string name, string tenantId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Units.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Name == name && u.TenantId == tenantId, cancellationToken);
            
        return MapToBO(entity);
    }

    public async Task<List<UnitBO>> GetAllUnitsAsync(string tenantId, CancellationToken cancellationToken)
    {
        var entities = await _dbContext.Units.AsNoTracking()
            .Where(u => u.TenantId == tenantId)
            .OrderBy(u => u.Name)
            .ToListAsync(cancellationToken);
            
        return entities.Select(MapToBO).Where(x => x != null).Cast<UnitBO>().ToList();
    }

    public async Task<UnitBO> CreateUnitAsync(UnitBO unit, CancellationToken cancellationToken)
    {
        var entity = new UnitEntity
        {
            Id = unit.Id,
            TenantId = unit.TenantId,
            Name = unit.Name,
            ShortName = unit.ShortName,
            IsActive = unit.IsActive
        };

        _dbContext.Units.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return unit;
    }

    public async Task<UnitBO> UpdateUnitAsync(UnitBO unit, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Units
            .FirstOrDefaultAsync(u => u.Id == unit.Id && u.TenantId == unit.TenantId, cancellationToken);

        if (entity != null)
        {
            entity.Name = unit.Name;
            entity.ShortName = unit.ShortName;
            entity.IsActive = unit.IsActive;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return unit;
    }

    private UnitBO? MapToBO(UnitEntity? entity)
    {
        if (entity == null) return null;
        
        return new UnitBO
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Name = entity.Name,
            ShortName = entity.ShortName,
            IsActive = entity.IsActive
        };
    }
}
