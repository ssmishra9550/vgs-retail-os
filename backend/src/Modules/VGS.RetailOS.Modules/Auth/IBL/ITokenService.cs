using VGS.RetailOS.Modules.Auth.BO;

namespace VGS.RetailOS.Modules.Auth.IBL;

/// <summary>
/// Framework-independent contract for token generation, hashing, and access-token creation.
/// Decouples AuthBL from concrete cryptographic hashing and JWT infrastructure libraries.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Computes the cryptographic hash (e.g. SHA-256) of a raw refresh token string for persistence lookup.
    /// </summary>
    string HashToken(string token);

    /// <summary>
    /// Generates a cryptographically random raw refresh token string along with its cryptographic hash.
    /// </summary>
    (string RawToken, string TokenHash) GenerateRefreshToken();

    /// <summary>
    /// Generates a signed access token (JWT) for the specified user and optional tenant context.
    /// </summary>
    (string AccessToken, DateTimeOffset ExpiresAt) GenerateAccessToken(UserBO user, string? tenantHint = null);
}
