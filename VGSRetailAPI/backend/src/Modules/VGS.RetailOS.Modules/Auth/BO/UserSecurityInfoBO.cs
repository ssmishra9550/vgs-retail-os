namespace VGS.RetailOS.Modules.Auth.BO;

public sealed class UserSecurityInfoBO
{
    public UserBO User { get; init; } = default!;
    public string PasswordHash { get; init; } = string.Empty;
    public int AccessFailedCount { get; init; }
    public DateTimeOffset? LockoutEnd { get; init; }

    public bool IsLockedOut(DateTimeOffset now) => LockoutEnd.HasValue && LockoutEnd.Value > now;
}
