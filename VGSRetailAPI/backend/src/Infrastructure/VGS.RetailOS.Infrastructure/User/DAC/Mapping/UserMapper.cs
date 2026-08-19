using VGS.RetailOS.Infrastructure.Auth.DAC.Entities;
using VGS.RetailOS.Modules.User.BO;

namespace VGS.RetailOS.Infrastructure.User.DAC.Mapping;

public static class UserMapper
{
    public static UserBO ToUserBO(this ApplicationUser entity)
    {
        return new UserBO
        {
            Id = entity.Id,
            Email = entity.Email ?? string.Empty,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            IsActive = entity.IsActive,
            LastLoginAt = entity.LastLoginAt,
            CreatedAt = entity.CreatedAt
        };
    }
}
