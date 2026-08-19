using VGS.RetailOS.Contracts.V1.Store.Requests;
using VGS.RetailOS.Contracts.V1.Store.Responses;

namespace VGS.RetailOS.Modules.Store.IBL;

public interface IStoreBL
{
    Task<StoreResponse> CreateAsync(CreateStoreRequest request, CancellationToken cancellationToken = default);
    Task<StoreResponse> UpdateAsync(Guid id, UpdateStoreRequest request, CancellationToken cancellationToken = default);
    Task<StoreResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<StoreResponse>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<List<StoreResponse>> GetAllForTenantAsync(CancellationToken cancellationToken = default);
}
