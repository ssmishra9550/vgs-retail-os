using VGS.RetailOS.Modules.Auth.BO;
using VGS.RetailOS.Modules.Auth.IBL;
using VGS.RetailOS.Modules.Auth.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;

namespace VGS.RetailOS.Modules.Auth.BL;

/// <summary>
/// Domain implementation of authentication business logic (AuthBL).
/// Strictly adheres to ADR-004 and ADR-005 canonical layering (API -> IBL -> BL -> BO / IDAC -> DAC -> EF Core -> PostgreSQL).
/// Framework-independent domain orchestrator: Zero dependencies on ASP.NET Core, EF Core, or Infrastructure.
/// </summary>
public class AuthBL : IAuthBL
{
    private const int MaxFailedAccessAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

    private readonly IAuthDAC _authDac;
    private readonly IPasswordVerifier _passwordVerifier;
    private readonly ITokenService _tokenService;

    public AuthBL(
        IAuthDAC authDac,
        IPasswordVerifier passwordVerifier,
        ITokenService tokenService)
    {
        _authDac = authDac ?? throw new ArgumentNullException(nameof(authDac));
        _passwordVerifier = passwordVerifier ?? throw new ArgumentNullException(nameof(passwordVerifier));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    }

    public async Task<AuthTokenResultBO> LoginAsync(LoginCommandBO command, CancellationToken cancellationToken = default)
    {
        if (command == null)
        {
            throw new ValidationException("Login command cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            throw new ValidationException("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Password))
        {
            throw new ValidationException("Password is required.");
        }

        // If TenantHint is provided, evaluate tenant membership.
        // SECURITY RULE: Because the Tenant & Membership module is deferred, VerifyTenantMembershipAsync
        // throws NotSupportedException. We catch it and fail safely via a domain ValidationException
        // to ensure login with a TenantHint never silently succeeds.
        if (!string.IsNullOrWhiteSpace(command.TenantHint))
        {
            try
            {
                await _authDac.VerifyTenantMembershipAsync(Guid.Empty, command.TenantHint, cancellationToken);
            }
            catch (NotSupportedException)
            {
                throw new ValidationException("Tenant-scoped authentication is not supported until the Tenant & Membership module is implemented.");
            }
        }

        var securityInfo = await _authDac.FindUserSecurityInfoByEmailAsync(command.Email, cancellationToken);

        // Security rule: Never reveal whether an email/user exists to prevent user enumeration attacks.
        if (securityInfo == null)
        {
            _passwordVerifier.VerifyPassword("dummy_hash_for_timing_mitigation", command.Password);
            throw new ValidationException("Invalid email or password.");
        }

        var now = DateTimeOffset.UtcNow;

        // Verify account active status
        if (!securityInfo.User.IsActive)
        {
            throw new ValidationException("Invalid email or password.");
        }

        // Verify account lockout status
        if (securityInfo.IsLockedOut(now))
        {
            throw new ValidationException("Account is locked due to multiple failed login attempts. Please try again later.");
        }

        // Verify password
        var verificationResult = _passwordVerifier.VerifyPassword(securityInfo.PasswordHash, command.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            var newFailedCount = securityInfo.AccessFailedCount + 1;

            if (newFailedCount >= MaxFailedAccessAttempts)
            {
                var lockoutEnd = now.Add(LockoutDuration);
                await _authDac.RecordLoginFailureAsync(securityInfo.User.Id, newFailedCount, lockoutEnd, cancellationToken);
                throw new ValidationException("Account has been locked due to 5 consecutive failed login attempts. Please try again in 15 minutes.");
            }

            await _authDac.RecordLoginFailureAsync(securityInfo.User.Id, newFailedCount, null, cancellationToken);
            throw new ValidationException("Invalid email or password.");
        }

        // Successful authentication
        await _authDac.RecordLoginSuccessAsync(securityInfo.User.Id, now, cancellationToken);

        var (accessToken, accessTokenExpiresAt) = _tokenService.GenerateAccessToken(securityInfo.User, command.TenantHint);
        var (rawRefreshToken, tokenHash) = _tokenService.GenerateRefreshToken();

        var familyId = Guid.NewGuid();
        var refreshTokenExpiresAt = now.Add(RefreshTokenLifetime);

        var refreshTokenBO = new RefreshTokenBO
        {
            Id = Guid.NewGuid(),
            UserId = securityInfo.User.Id,
            TokenHash = tokenHash,
            FamilyId = familyId,
            ExpiresAt = refreshTokenExpiresAt,
            IsRevoked = false,
            ReplacedByTokenHash = null,
            CreatedFromIp = command.CreatedFromIp,
            UserAgent = command.UserAgent,
            CreatedAt = now,
            RevokedAt = null,
            RevocationReason = null
        };

        await _authDac.SaveRefreshTokenAsync(refreshTokenBO, cancellationToken);

        return new AuthTokenResultBO
        {
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            RefreshToken = rawRefreshToken,
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
            User = securityInfo.User
        };
    }

    public async Task<AuthTokenResultBO> RefreshTokenAsync(string refreshToken, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new ValidationException("Refresh token is required.");
        }

        var tokenHash = _tokenService.HashToken(refreshToken);
        var existingToken = await _authDac.GetRefreshTokenByHashAsync(tokenHash, cancellationToken);

        if (existingToken == null)
        {
            throw new ValidationException("Invalid or expired refresh token.");
        }

        // SECURITY REUSE DETECTION: If an already-revoked refresh token is presented, revoke the entire token family immediately.
        if (existingToken.IsRevoked)
        {
            await _authDac.RevokeTokenFamilyAsync(existingToken.FamilyId, "Security Alert: Refresh token reuse detected.", cancellationToken);
            throw new ValidationException("Security Alert: Invalid refresh token. Session terminated.");
        }

        var now = DateTimeOffset.UtcNow;

        if (existingToken.IsExpired(now))
        {
            await _authDac.RevokeRefreshTokenAsync(tokenHash, "Expired", cancellationToken);
            throw new ValidationException("Refresh token has expired. Please log in again.");
        }

        var user = await _authDac.GetUserByIdAsync(existingToken.UserId, cancellationToken);

        if (user == null || !user.IsActive)
        {
            await _authDac.RevokeRefreshTokenAsync(tokenHash, "User inactive or deleted", cancellationToken);
            throw new ValidationException("User account is inactive or no longer exists.");
        }

        // Rotate refresh token
        var (newAccessToken, accessTokenExpiresAt) = _tokenService.GenerateAccessToken(user);
        var (newRawRefreshToken, newTokenHash) = _tokenService.GenerateRefreshToken();

        var newRefreshTokenExpiresAt = now.Add(RefreshTokenLifetime);

        var newRefreshTokenBO = new RefreshTokenBO
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = newTokenHash,
            FamilyId = existingToken.FamilyId,
            ExpiresAt = newRefreshTokenExpiresAt,
            IsRevoked = false,
            ReplacedByTokenHash = null,
            CreatedFromIp = ipAddress,
            UserAgent = userAgent,
            CreatedAt = now,
            RevokedAt = null,
            RevocationReason = null
        };

        try
        {
            await _authDac.RotateRefreshTokenAsync(tokenHash, newRefreshTokenBO, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            // SECURITY CONCURRENCY PROTECTION: Handle secondary concurrent rotation requests safely without 500 errors.
            // Revoke the token family to protect against potential replay attacks and fail with a safe ValidationException.
            await _authDac.RevokeTokenFamilyAsync(existingToken.FamilyId, "Security Alert: Concurrent token rotation conflict or replay attempt.", cancellationToken);
            throw new ValidationException("Security Alert: Refresh token has already been rotated or revoked. Session terminated.");
        }

        return new AuthTokenResultBO
        {
            AccessToken = newAccessToken,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            RefreshToken = newRawRefreshToken,
            RefreshTokenExpiresAt = newRefreshTokenExpiresAt,
            User = user
        };
    }

    public async Task LogoutAsync(string refreshToken, string? reason, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        var tokenHash = _tokenService.HashToken(refreshToken);
        await _authDac.RevokeRefreshTokenAsync(tokenHash, reason ?? "User logged out", cancellationToken);
    }

    public async Task<UserBO?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            return null;
        }

        return await _authDac.GetUserByIdAsync(userId, cancellationToken);
    }
}
