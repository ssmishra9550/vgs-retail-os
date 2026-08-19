using VGS.RetailOS.Infrastructure.Auth.DAC.Entities;

namespace VGS.RetailOS.Infrastructure.User.DAC.Entities;

public class TenantUserMembershipEntity
{
    public Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required string TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public virtual ApplicationUser User { get; set; } = default!;
}
