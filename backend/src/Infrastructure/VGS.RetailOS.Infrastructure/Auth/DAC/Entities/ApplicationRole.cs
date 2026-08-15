using Microsoft.AspNetCore.Identity;

namespace VGS.RetailOS.Infrastructure.Auth.DAC.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
