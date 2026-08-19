using VGS.RetailOS.Contracts.V1.Role.Requests;
using VGS.RetailOS.Contracts.V1.Role.Responses;
using VGS.RetailOS.Modules.Role.BO;
using VGS.RetailOS.Modules.Role.IBL;
using VGS.RetailOS.Modules.Role.IDAC;
using VGS.RetailOS.Modules.User.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;

namespace VGS.RetailOS.Modules.Role.BL;

public class RoleBL : IRoleBL
{
    private readonly IRoleDAC _roleDac;
    private readonly IUserDAC _userDac;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public RoleBL(IRoleDAC roleDac, IUserDAC userDac, ITenantContextAccessor tenantContextAccessor)
    {
        _roleDac = roleDac ?? throw new ArgumentNullException(nameof(roleDac));
        _userDac = userDac ?? throw new ArgumentNullException(nameof(userDac));
        _tenantContextAccessor = tenantContextAccessor ?? throw new ArgumentNullException(nameof(tenantContextAccessor));
    }

    public async Task<List<RoleResponse>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var roles = await _roleDac.GetRolesAsync(tenantId, cancellationToken);
        return roles.Select(MapToResponse).ToList();
    }

    public async Task<RoleResponse> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var role = await _roleDac.GetRoleByIdAsync(id, tenantId, cancellationToken);
        
        if (role == null || (!role.IsSystemRole && role.TenantId != tenantId))
        {
            throw new NotFoundException($"Role with ID {id} not found.");
        }

        return MapToResponse(role);
    }

    public async Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tenantId = GetTenantId();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Role Name is required.");
        }

        var roleBo = new RoleBO
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            TenantId = tenantId,
            IsSystemRole = false,
            Permissions = request.Permissions ?? [],
            CreatedAt = DateTimeOffset.UtcNow
        };

        var createdRole = await _roleDac.CreateRoleAsync(roleBo, cancellationToken);
        return MapToResponse(createdRole);
    }

    public async Task AssignRoleToUserAsync(Guid roleId, AssignRoleRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tenantId = GetTenantId();

        var role = await _roleDac.GetRoleByIdAsync(roleId, tenantId, cancellationToken);
        if (role == null || (!role.IsSystemRole && role.TenantId != tenantId))
        {
            throw new NotFoundException($"Role with ID {roleId} not found.");
        }

        // Validate that the user is part of the tenant
        var userInTenant = await _userDac.IsUserInTenantAsync(request.UserId, tenantId, cancellationToken);
        if (!userInTenant)
        {
            throw new ConflictException($"User with ID {request.UserId} is not a member of the current tenant.");
        }

        await _roleDac.AssignRoleToUserAsync(request.UserId, roleId, cancellationToken);
    }

    private string GetTenantId()
    {
        var tenantContext = _tenantContextAccessor.TenantContext;
        if (tenantContext == null || string.IsNullOrWhiteSpace(tenantContext.CurrentTenantId))
        {
            throw new TenantNotFoundException("A valid tenant context is required to perform role operations.");
        }
        return tenantContext.CurrentTenantId;
    }

    private static RoleResponse MapToResponse(RoleBO bo)
    {
        return new RoleResponse
        {
            Id = bo.Id,
            Name = bo.Name,
            Description = bo.Description,
            IsSystemRole = bo.IsSystemRole,
            Permissions = bo.Permissions,
            CreatedAt = bo.CreatedAt
        };
    }
}
