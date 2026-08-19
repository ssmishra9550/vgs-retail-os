namespace VGS.RetailOS.Modules.Auth.BO;

/// <summary>
/// Domain model representing persistent refresh token session state.
/// SECURITY MANDATE: RefreshTokenBO NEVER contains raw plaintext tokens.
/// Only cryptographic TokenHash is held and persisted.
/// </summary>
public sealed class RefreshTokenBO
{
    public Guid Id { get; init; }
    
    /// <summary>
    /// Cryptographic hash of the refresh token. Plaintext refresh tokens are NEVER stored here.
    /// </summary>
    public string TokenHash { get; init; } = string.Empty;
    
    public Guid UserId { get; init; }
    public Guid FamilyId { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
    public bool IsRevoked { get; init; }
    public string? ReplacedByTokenHash { get; init; }
    public string? CreatedFromIp { get; init; }
    public string? UserAgent { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? RevokedAt { get; init; }
    public string? RevocationReason { get; init; }

    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;

    public bool IsActive(DateTimeOffset now) => !IsRevoked && !IsExpired(now);
}
