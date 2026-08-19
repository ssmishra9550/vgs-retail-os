using VGS.RetailOS.Infrastructure.Auth.DAC.Entities;
using VGS.RetailOS.Modules.Role.BO;

namespace VGS.RetailOS.Infrastructure.Role.DAC.Mapping;

public static class RoleMapper
{
    public static RoleBO ToRoleBO(this ApplicationRole entity)
    {
        return new RoleBO
        {
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            Description = entity.Description,
            IsSystemRole = entity.IsSystemRole,
            TenantId = entity.TenantId,
            Permissions = entity.Permissions,
            CreatedAt = entity.CreatedAt
        };
    }
}
