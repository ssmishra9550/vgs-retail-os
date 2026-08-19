using Microsoft.EntityFrameworkCore;
using VGS.RetailOS.Infrastructure.Auth.DAC.Entities;
using VGS.RetailOS.Infrastructure.Data;
using VGS.RetailOS.Infrastructure.User.DAC.Entities;
using VGS.RetailOS.Infrastructure.User.DAC.Mapping;
using VGS.RetailOS.Modules.User.BO;
using VGS.RetailOS.Modules.User.IDAC;

namespace VGS.RetailOS.Infrastructure.User.DAC;

public class UserDAC : IUserDAC
{
    private readonly AppDbContext _dbContext;

    public UserDAC(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<UserBO?> GetByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Join(_dbContext.TenantUserMemberships.Where(m => m.TenantId == tenantId && m.IsActive),
                u => u.Id,
                m => m.UserId,
                (u, m) => u)
            .FirstOrDefaultAsync(cancellationToken);

        return user?.ToUserBO();
    }

    public async Task<UserBO?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToUpperInvariant();
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
            
        return user?.ToUserBO();
    }

    public async Task<List<UserBO>> GetAllInTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var users = await _dbContext.Users
            .AsNoTracking()
            .Join(_dbContext.TenantUserMemberships.Where(m => m.TenantId == tenantId && m.IsActive),
                u => u.Id,
                m => m.UserId,
                (u, m) => u)
            .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
            .ToListAsync(cancellationToken);

        return users.Select(u => u.ToUserBO()).ToList();
    }

    public async Task<UserBO> CreateUserAsync(UserBO user, string passwordHash, CancellationToken cancellationToken = default)
    {
        var entity = new ApplicationUser
        {
            Id = user.Id,
            UserName = user.Email,
            NormalizedUserName = user.Email.ToUpperInvariant(),
            Email = user.Email,
            NormalizedEmail = user.Email.ToUpperInvariant(),
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            PasswordHash = passwordHash,
            SecurityStamp = Guid.NewGuid().ToString(),
            CreatedAt = user.CreatedAt
        };

        _dbContext.Users.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.ToUserBO();
    }

    public async Task AddUserToTenantAsync(Guid userId, string tenantId, CancellationToken cancellationToken = default)
    {
        var membershipExists = await _dbContext.TenantUserMemberships
            .IgnoreQueryFilters()
            .AnyAsync(m => m.UserId == userId && m.TenantId == tenantId, cancellationToken);

        if (!membershipExists)
        {
            var membership = new TenantUserMembershipEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TenantId = tenantId,
                IsActive = true,
                JoinedAt = DateTimeOffset.UtcNow
            };

            _dbContext.TenantUserMemberships.Add(membership);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> IsUserInTenantAsync(Guid userId, string tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TenantUserMemberships
            .IgnoreQueryFilters()
            .AnyAsync(m => m.UserId == userId && m.TenantId == tenantId && m.IsActive, cancellationToken);
    }
}
