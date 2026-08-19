using VGS.RetailOS.Contracts.V1.Role.Requests;
using VGS.RetailOS.Contracts.V1.Role.Responses;

namespace VGS.RetailOS.Modules.Role.IBL;

public interface IRoleBL
{
    Task<List<RoleResponse>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<RoleResponse> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);
    Task AssignRoleToUserAsync(Guid roleId, AssignRoleRequest request, CancellationToken cancellationToken = default);
}
