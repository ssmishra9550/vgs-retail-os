using VGS.RetailOS.Modules.Store.BO;

namespace VGS.RetailOS.Modules.Store.IDAC;

public interface IStoreDAC
{
    Task<StoreBO> CreateAsync(StoreBO store, CancellationToken cancellationToken = default);
    Task<StoreBO> UpdateAsync(StoreBO store, CancellationToken cancellationToken = default);
    Task<StoreBO?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<StoreBO>> GetByOrganizationIdAsync(Guid organizationId, string tenantId, CancellationToken cancellationToken = default);
    Task<List<StoreBO>> GetAllForTenantAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, Guid organizationId, string tenantId, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, Guid organizationId, string tenantId, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
