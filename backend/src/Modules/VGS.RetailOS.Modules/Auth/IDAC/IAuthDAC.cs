using VGS.RetailOS.Modules.Auth.BO;

namespace VGS.RetailOS.Modules.Auth.IDAC;

public interface IAuthDAC
{
    Task<UserSecurityInfoBO?> FindUserSecurityInfoByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<UserBO?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> VerifyTenantMembershipAsync(Guid userId, string tenantIdentifier, CancellationToken cancellationToken = default);

    Task RecordLoginSuccessAsync(Guid userId, DateTimeOffset loginTime, CancellationToken cancellationToken = default);

    Task RecordLoginFailureAsync(Guid userId, int newAccessFailedCount, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken = default);

    Task<RefreshTokenBO?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task SaveRefreshTokenAsync(RefreshTokenBO token, CancellationToken cancellationToken = default);

    Task RotateRefreshTokenAsync(string oldTokenHash, RefreshTokenBO newToken, CancellationToken cancellationToken = default);

    Task RevokeTokenFamilyAsync(Guid familyId, string reason, CancellationToken cancellationToken = default);

    Task RevokeRefreshTokenAsync(string tokenHash, string reason, CancellationToken cancellationToken = default);
}
