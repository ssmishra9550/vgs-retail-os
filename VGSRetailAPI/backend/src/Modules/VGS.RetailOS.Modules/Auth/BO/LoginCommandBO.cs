namespace VGS.RetailOS.Modules.Auth.BO;

/// <summary>
/// Transient authentication command object carrying user login credentials.
/// SECURITY MANDATE: Password is a transient credential held ONLY in memory for the duration
/// of authentication processing. It MUST NEVER be logged, persisted, attached to UserBO,
/// returned in API responses, or exposed in exceptions.
/// </summary>
public sealed class LoginCommandBO
{
    public string? TenantHint { get; init; }
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Transient raw password credential. Memory-only lifetime during authentication.
    /// </summary>
    public string Password { get; init; } = string.Empty;

    public string? CreatedFromIp { get; init; }
    public string? UserAgent { get; init; }
}
