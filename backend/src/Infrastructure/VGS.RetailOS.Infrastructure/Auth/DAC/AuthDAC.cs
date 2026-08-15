using Microsoft.EntityFrameworkCore;
using VGS.RetailOS.Infrastructure.Auth.DAC.Entities;
using VGS.RetailOS.Infrastructure.Auth.DAC.Mapping;
using VGS.RetailOS.Infrastructure.Data;
using VGS.RetailOS.Modules.Auth.BO;
using VGS.RetailOS.Modules.Auth.IDAC;

namespace VGS.RetailOS.Infrastructure.Auth.DAC;

public class AuthDAC : IAuthDAC
{
    private readonly AppDbContext _dbContext;

    public AuthDAC(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<UserSecurityInfoBO?> FindUserSecurityInfoByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var normalizedEmail = email.Trim().ToUpperInvariant();

        var userEntity = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);

        return userEntity?.ToUserSecurityInfoBO();
    }

    public async Task<UserBO?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userEntity = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return userEntity?.ToUserBO();
    }

    /// <summary>
    /// Verifies whether the specified user has valid membership in the given target tenant.
    /// SECURITY RULE: Throws NotSupportedException to prevent silent security bypasses until
    /// the Tenant & Membership module is fully implemented in a later architectural phase.
    /// </summary>
    public Task<bool> VerifyTenantMembershipAsync(Guid userId, string tenantIdentifier, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException(
            "Tenant membership verification is not supported until the Tenant/Membership domain module is implemented. Silent authorization success is prohibited.");
    }

    public async Task RecordLoginSuccessAsync(Guid userId, DateTimeOffset loginTime, CancellationToken cancellationToken = default)
    {
        var userEntity = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (userEntity == null)
        {
            return;
        }

        userEntity.LastLoginAt = loginTime;
        userEntity.AccessFailedCount = 0;
        userEntity.LockoutEnd = null;
        userEntity.UpdatedAt = loginTime;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RecordLoginFailureAsync(Guid userId, int newAccessFailedCount, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken = default)
    {
        var userEntity = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (userEntity == null)
        {
            return;
        }

        userEntity.AccessFailedCount = newAccessFailedCount;
        userEntity.LockoutEnd = lockoutEnd;
        userEntity.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<RefreshTokenBO?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            return null;
        }

        var tokenEntity = await _dbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash, cancellationToken);

        return tokenEntity?.ToRefreshTokenBO();
    }

    public async Task SaveRefreshTokenAsync(RefreshTokenBO token, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(token);

        var entity = token.ToEntity();
        _dbContext.RefreshTokens.Add(entity);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Rotates a refresh token using an atomic EF Core conditional update inside a database transaction.
    /// ATOMIC CONCURRENCY GUARANTEE: Uses ExecuteUpdateAsync with `WHERE TokenHash = @oldTokenHash AND IsRevoked = false`.
    /// Concurrent rotation attempts of the same token will execute atomically; exactly one will update 1 row,
    /// while all secondary concurrent attempts observe 0 affected rows, rollback, and fail safely without creating a replacement token.
    /// SECURITY RULE: Exception messages use generic security responses and NEVER expose token hashes.
    /// </summary>
    public async Task RotateRefreshTokenAsync(string oldTokenHash, RefreshTokenBO newToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(oldTokenHash);
        ArgumentNullException.ThrowIfNull(newToken);

        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var rowsAffected = await _dbContext.RefreshTokens
            .Where(r => r.TokenHash == oldTokenHash && !r.IsRevoked)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.IsRevoked, true)
                .SetProperty(r => r.RevokedAt, DateTimeOffset.UtcNow)
                .SetProperty(r => r.ReplacedByTokenHash, newToken.TokenHash)
                .SetProperty(r => r.RevocationReason, "Rotated"),
                cancellationToken);

        if (rowsAffected == 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new InvalidOperationException("Security Violation: Refresh token has already been rotated, revoked, or does not exist.");
        }

        var newTokenEntity = newToken.ToEntity();
        _dbContext.RefreshTokens.Add(newTokenEntity);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task RevokeTokenFamilyAsync(Guid familyId, string reason, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        await _dbContext.RefreshTokens
            .Where(r => r.FamilyId == familyId && !r.IsRevoked)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.IsRevoked, true)
                .SetProperty(r => r.RevokedAt, now)
                .SetProperty(r => r.RevocationReason, reason),
                cancellationToken);
    }

    public async Task RevokeRefreshTokenAsync(string tokenHash, string reason, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            return;
        }

        var tokenEntity = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash, cancellationToken);

        if (tokenEntity == null || tokenEntity.IsRevoked)
        {
            return;
        }

        tokenEntity.IsRevoked = true;
        tokenEntity.RevokedAt = DateTimeOffset.UtcNow;
        tokenEntity.RevocationReason = reason;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
