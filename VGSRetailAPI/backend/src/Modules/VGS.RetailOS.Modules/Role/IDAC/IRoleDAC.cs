using VGS.RetailOS.Modules.Role.BO;

namespace VGS.RetailOS.Modules.Role.IDAC;

public interface IRoleDAC
{
    Task<List<RoleBO>> GetRolesAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<RoleBO?> GetRoleByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken = default);
    Task<RoleBO> CreateRoleAsync(RoleBO role, CancellationToken cancellationToken = default);
    Task AssignRoleToUserAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
    Task<bool> IsRoleAssignedToUserAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
}
