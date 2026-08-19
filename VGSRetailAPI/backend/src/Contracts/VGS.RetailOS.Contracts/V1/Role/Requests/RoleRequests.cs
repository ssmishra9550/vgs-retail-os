namespace VGS.RetailOS.Contracts.V1.Role.Requests;

public class CreateRoleRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string[] Permissions { get; set; } = [];
}

public class AssignRoleRequest
{
    public required Guid UserId { get; set; }
}
