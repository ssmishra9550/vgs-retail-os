using Microsoft.EntityFrameworkCore;
using VGS.RetailOS.Infrastructure.Data;
using VGS.RetailOS.Infrastructure.Organization.DAC.Mapping;
using VGS.RetailOS.Modules.Organization.BO;
using VGS.RetailOS.Modules.Organization.IDAC;

namespace VGS.RetailOS.Infrastructure.Organization.DAC;

public class OrganizationDAC : IOrganizationDAC
{
    private readonly AppDbContext _dbContext;

    public OrganizationDAC(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrganizationBO> CreateAsync(OrganizationBO organization, CancellationToken cancellationToken = default)
    {
        var entity = OrganizationMapper.ToEntity(organization);
        
        await _dbContext.Organizations.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return OrganizationMapper.ToBo(entity);
    }

    public async Task<OrganizationBO?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Global query filter ensures we only get organizations for the current tenant
        var entity = await _dbContext.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        return entity == null ? null : OrganizationMapper.ToBo(entity);
    }

    public async Task<OrganizationBO> UpdateAsync(OrganizationBO organization, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Organizations
            .FirstOrDefaultAsync(o => o.Id == organization.Id, cancellationToken);

        if (entity == null)
        {
            throw new InvalidOperationException($"Organization {organization.Id} not found or access denied.");
        }

        entity.Name = organization.Name;
        entity.Code = organization.Code;
        entity.TaxId = organization.TaxId;
        entity.Address = organization.Address;
        entity.ContactEmail = organization.ContactEmail;
        entity.ContactPhone = organization.ContactPhone;
        entity.UpdatedAt = organization.UpdatedAt;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return OrganizationMapper.ToBo(entity);
    }

    public async Task<bool> ExistsByNameAsync(string name, string tenantId, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Organizations.AsNoTracking().Where(o => o.Name == name && o.TenantId == tenantId);
        
        if (excludeId.HasValue)
        {
            query = query.Where(o => o.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
