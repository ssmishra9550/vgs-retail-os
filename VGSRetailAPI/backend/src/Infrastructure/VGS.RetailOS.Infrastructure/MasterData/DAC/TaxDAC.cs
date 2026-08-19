using Microsoft.EntityFrameworkCore;
using VGS.RetailOS.Infrastructure.Data;
using VGS.RetailOS.Infrastructure.MasterData.DAC.Entities;
using VGS.RetailOS.Modules.MasterData.Tax.BO;
using VGS.RetailOS.Modules.MasterData.Tax.IDAC;

namespace VGS.RetailOS.Infrastructure.MasterData.DAC;

public class TaxDAC : ITaxDAC
{
    private readonly AppDbContext _dbContext;

    public TaxDAC(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TaxBO?> GetTaxByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Taxes.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId, cancellationToken);
            
        return MapToBO(entity);
    }

    public async Task<TaxBO?> GetTaxByNameAsync(string name, string tenantId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Taxes.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Name == name && t.TenantId == tenantId, cancellationToken);
            
        return MapToBO(entity);
    }

    public async Task<List<TaxBO>> GetAllTaxesAsync(string tenantId, CancellationToken cancellationToken)
    {
        var entities = await _dbContext.Taxes.AsNoTracking()
            .Where(t => t.TenantId == tenantId)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
            
        return entities.Select(MapToBO).Where(x => x != null).Cast<TaxBO>().ToList();
    }

    public async Task<TaxBO> CreateTaxAsync(TaxBO tax, CancellationToken cancellationToken)
    {
        var entity = new TaxEntity
        {
            Id = tax.Id,
            TenantId = tax.TenantId,
            Name = tax.Name,
            Rate = tax.Rate,
            Type = Enum.Parse<TaxType>(tax.Type),
            IsActive = tax.IsActive
        };

        _dbContext.Taxes.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return tax;
    }

    public async Task<TaxBO> UpdateTaxAsync(TaxBO tax, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Taxes
            .FirstOrDefaultAsync(t => t.Id == tax.Id && t.TenantId == tax.TenantId, cancellationToken);

        if (entity != null)
        {
            entity.Name = tax.Name;
            entity.Rate = tax.Rate;
            entity.Type = Enum.Parse<TaxType>(tax.Type);
            entity.IsActive = tax.IsActive;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return tax;
    }

    private TaxBO? MapToBO(TaxEntity? entity)
    {
        if (entity == null) return null;
        
        return new TaxBO
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Name = entity.Name,
            Rate = entity.Rate,
            Type = entity.Type.ToString(),
            IsActive = entity.IsActive
        };
    }
}
