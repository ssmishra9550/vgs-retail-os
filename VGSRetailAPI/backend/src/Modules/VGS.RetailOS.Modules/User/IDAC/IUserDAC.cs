using VGS.RetailOS.Modules.User.BO;

namespace VGS.RetailOS.Modules.User.IDAC;

public interface IUserDAC
{
    Task<UserBO?> GetByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken = default);
    Task<UserBO?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<List<UserBO>> GetAllInTenantAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<UserBO> CreateUserAsync(UserBO user, string passwordHash, CancellationToken cancellationToken = default);
    Task AddUserToTenantAsync(Guid userId, string tenantId, CancellationToken cancellationToken = default);
    Task<bool> IsUserInTenantAsync(Guid userId, string tenantId, CancellationToken cancellationToken = default);
}
