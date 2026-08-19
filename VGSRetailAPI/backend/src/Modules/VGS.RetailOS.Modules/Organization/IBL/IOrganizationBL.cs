using VGS.RetailOS.Contracts.V1.Organization.Requests;
using VGS.RetailOS.Contracts.V1.Organization.Responses;

namespace VGS.RetailOS.Modules.Organization.IBL;

public interface IOrganizationBL
{
    Task<OrganizationResponse> CreateAsync(CreateOrganizationRequest request, CancellationToken cancellationToken = default);
    Task<OrganizationResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrganizationResponse> UpdateAsync(Guid id, UpdateOrganizationRequest request, CancellationToken cancellationToken = default);
}
