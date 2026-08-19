namespace VGS.RetailOS.Contracts.V1.Store.Requests;

public class CreateStoreRequest
{
    public required Guid OrganizationId { get; set; }
    public required string Name { get; set; }
    public string? Code { get; set; }
    public string? Address { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
}
