namespace VGS.RetailOS.Contracts.V1.Store.Responses;

public class StoreResponse
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public required string Name { get; set; }
    public string? Code { get; set; }
    public string? Address { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
