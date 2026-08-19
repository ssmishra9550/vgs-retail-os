using VGS.RetailOS.Contracts.V1.User.Requests;
using VGS.RetailOS.Contracts.V1.User.Responses;

namespace VGS.RetailOS.Modules.User.IBL;

public interface IUserBL
{
    Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<List<UserResponse>> GetUsersInTenantAsync(CancellationToken cancellationToken = default);
    Task<UserResponse> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
