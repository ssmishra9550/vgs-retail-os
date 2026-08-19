namespace VGS.RetailOS.Contracts.V1.Organization.Requests;

public class CreateOrganizationRequest
{
    public required string Name { get; init; }
    public string? Code { get; init; }
    public string? TaxId { get; init; }
    public string? Address { get; init; }
    public string? ContactEmail { get; init; }
    public string? ContactPhone { get; init; }
}
