using VGS.RetailOS.Modules.Organization.BO;

namespace VGS.RetailOS.Modules.Organization.IDAC;

public interface IOrganizationDAC
{
    Task<OrganizationBO> CreateAsync(OrganizationBO organization, CancellationToken cancellationToken = default);
    Task<OrganizationBO?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrganizationBO> UpdateAsync(OrganizationBO organization, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, string tenantId, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
