using VGS.RetailOS.Infrastructure.Auth.DAC.Entities;
using VGS.RetailOS.Modules.Auth.BO;

namespace VGS.RetailOS.Infrastructure.Auth.DAC.Mapping;

public static class AuthMappingExtensions
{
    public static UserBO ToUserBO(this ApplicationUser entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new UserBO
        {
            Id = entity.Id,
            Email = entity.Email ?? string.Empty,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            IsActive = entity.IsActive,
            MustChangePassword = entity.MustChangePassword,
            SecurityStamp = entity.SecurityStamp ?? string.Empty,
            LastLoginAt = entity.LastLoginAt,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public static UserSecurityInfoBO ToUserSecurityInfoBO(this ApplicationUser entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var userBO = entity.ToUserBO();

        return new UserSecurityInfoBO
        {
            User = userBO,
            PasswordHash = entity.PasswordHash ?? string.Empty,
            AccessFailedCount = entity.AccessFailedCount,
            LockoutEnd = entity.LockoutEnd
        };
    }

    public static RefreshTokenBO ToRefreshTokenBO(this RefreshTokenEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new RefreshTokenBO
        {
            Id = entity.Id,
            UserId = entity.UserId,
            TokenHash = entity.TokenHash,
            FamilyId = entity.FamilyId,
            ExpiresAt = entity.ExpiresAt,
            IsRevoked = entity.IsRevoked,
            ReplacedByTokenHash = entity.ReplacedByTokenHash,
            CreatedFromIp = entity.CreatedFromIp,
            UserAgent = entity.UserAgent,
            CreatedAt = entity.CreatedAt,
            RevokedAt = entity.RevokedAt,
            RevocationReason = entity.RevocationReason
        };
    }

    public static RefreshTokenEntity ToEntity(this RefreshTokenBO bo)
    {
        ArgumentNullException.ThrowIfNull(bo);

        return new RefreshTokenEntity
        {
            Id = bo.Id == Guid.Empty ? Guid.NewGuid() : bo.Id,
            UserId = bo.UserId,
            TokenHash = bo.TokenHash,
            FamilyId = bo.FamilyId,
            ExpiresAt = bo.ExpiresAt,
            IsRevoked = bo.IsRevoked,
            ReplacedByTokenHash = bo.ReplacedByTokenHash,
            CreatedFromIp = bo.CreatedFromIp,
            UserAgent = bo.UserAgent,
            CreatedAt = bo.CreatedAt,
            RevokedAt = bo.RevokedAt,
            RevocationReason = bo.RevocationReason
        };
    }
}
