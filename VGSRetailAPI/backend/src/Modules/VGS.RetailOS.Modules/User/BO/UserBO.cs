namespace VGS.RetailOS.Modules.User.BO;

public class UserBO
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
