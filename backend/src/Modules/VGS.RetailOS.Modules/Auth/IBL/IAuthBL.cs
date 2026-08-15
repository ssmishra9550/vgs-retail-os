using VGS.RetailOS.Modules.Auth.BO;

namespace VGS.RetailOS.Modules.Auth.IBL;

public interface IAuthBL
{
    Task<AuthTokenResultBO> LoginAsync(LoginCommandBO command, CancellationToken cancellationToken = default);

    Task<AuthTokenResultBO> RefreshTokenAsync(string refreshToken, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);

    Task LogoutAsync(string refreshToken, string? reason, CancellationToken cancellationToken = default);

    Task<UserBO?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
