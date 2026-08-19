namespace VGS.RetailOS.Modules.Auth.BO;

public sealed class UserBO
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
    public bool MustChangePassword { get; init; }
    public string SecurityStamp { get; init; } = string.Empty;
    public DateTimeOffset? LastLoginAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; init; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
