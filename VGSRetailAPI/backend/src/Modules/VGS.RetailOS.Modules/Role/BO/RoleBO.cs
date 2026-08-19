namespace VGS.RetailOS.Modules.Role.BO;

public class RoleBO
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public string? TenantId { get; set; }
    public string[] Permissions { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
}
