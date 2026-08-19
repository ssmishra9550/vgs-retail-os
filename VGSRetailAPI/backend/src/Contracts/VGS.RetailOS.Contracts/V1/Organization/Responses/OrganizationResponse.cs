namespace VGS.RetailOS.Contracts.V1.Organization.Responses;

public class OrganizationResponse
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Code { get; init; }
    public string? TaxId { get; init; }
    public string? Address { get; init; }
    public string? ContactEmail { get; init; }
    public string? ContactPhone { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
