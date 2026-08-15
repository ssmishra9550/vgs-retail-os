namespace VGS.RetailOS.Modules.Auth.BO;

/// <summary>
/// Transient business result returned from authentication use cases (Login/Refresh).
/// Explicitly represents the transient response contract delivered securely to API/clients.
/// SECURITY MANDATE: RefreshToken here is the transient plaintext token string intended for secure
/// HttpOnly transport. It does NOT represent persisted token domain state (which stores only TokenHash).
/// </summary>
public sealed class AuthTokenResultBO
{
    public string AccessToken { get; init; } = string.Empty;
    public DateTimeOffset AccessTokenExpiresAt { get; init; }
    
    /// <summary>
    /// Transient plaintext refresh token string returned once to caller for secure cookie delivery.
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;
    
    public DateTimeOffset RefreshTokenExpiresAt { get; init; }
    public UserBO User { get; init; } = default!;
}
