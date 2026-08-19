namespace VGS.RetailOS.Contracts.V1.Role.Responses;

public class RoleResponse
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public string[] Permissions { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
}
