using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VGS.RetailOS.Infrastructure.Auth.DAC.Entities;
using VGS.RetailOS.Infrastructure.Data;
using VGS.RetailOS.Infrastructure.Role.DAC.Mapping;
using VGS.RetailOS.Modules.Role.BO;
using VGS.RetailOS.Modules.Role.IDAC;

namespace VGS.RetailOS.Infrastructure.Role.DAC;

public class RoleDAC : IRoleDAC
{
    private readonly AppDbContext _dbContext;

    public RoleDAC(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<List<RoleBO>> GetRolesAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var roles = await _dbContext.Roles
            .AsNoTracking()
            // The global query filter automatically scopes this to the tenant OR IsSystemRole.
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        return roles.Select(r => r.ToRoleBO()).ToList();
    }

    public async Task<RoleBO?> GetRoleByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken = default)
    {
        var role = await _dbContext.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        return role?.ToRoleBO();
    }

    public async Task<RoleBO> CreateRoleAsync(RoleBO role, CancellationToken cancellationToken = default)
    {
        var entity = new ApplicationRole
        {
            Id = role.Id,
            Name = role.Name,
            NormalizedName = role.Name.ToUpperInvariant(),
            Description = role.Description,
            TenantId = role.TenantId,
            IsSystemRole = false,
            Permissions = role.Permissions,
            CreatedAt = role.CreatedAt
        };

        _dbContext.Roles.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.ToRoleBO();
    }

    public async Task AssignRoleToUserAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        var exists = await _dbContext.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);

        if (!exists)
        {
            _dbContext.UserRoles.Add(new IdentityUserRole<Guid>
            {
                UserId = userId,
                RoleId = roleId
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> IsRoleAssignedToUserAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);
    }
}
