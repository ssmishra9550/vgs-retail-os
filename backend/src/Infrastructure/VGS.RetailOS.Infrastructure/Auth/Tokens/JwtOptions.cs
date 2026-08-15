namespace VGS.RetailOS.Infrastructure.Auth.Tokens;

/// <summary>
/// Strongly typed configuration options for JWT Access Token generation and validation.
/// Enforces ADR-005 security constraints at startup (HS256 minimum key size, 15 min lifetime).
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Security:Jwt";

    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int ExpirationMinutes { get; init; } = 15;
    public string SecretKey { get; init; } = string.Empty;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SecretKey))
        {
            throw new InvalidOperationException("JWT SecretKey configuration is missing or empty.");
        }

        if (SecretKey.Length < 32)
        {
            throw new InvalidOperationException($"JWT SecretKey must provide at least 256 bits (32 characters) for HS256 security. Provided length: {SecretKey.Length}.");
        }

        if (string.IsNullOrWhiteSpace(Issuer))
        {
            throw new InvalidOperationException("JWT Issuer configuration is missing or empty.");
        }

        if (string.IsNullOrWhiteSpace(Audience))
        {
            throw new InvalidOperationException("JWT Audience configuration is missing or empty.");
        }

        if (ExpirationMinutes != 15)
        {
            throw new InvalidOperationException($"JWT ExpirationMinutes must be set to exactly 15 minutes per ADR-005. Configured value: {ExpirationMinutes}.");
        }
    }
}
