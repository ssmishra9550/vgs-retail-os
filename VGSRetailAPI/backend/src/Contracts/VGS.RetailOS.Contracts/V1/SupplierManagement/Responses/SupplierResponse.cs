namespace VGS.RetailOS.Contracts.V1.SupplierManagement.Responses;

public class SupplierResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? ContactPerson { get; set; }
    public string Mobile { get; set; } = null!;
    public string? Email { get; set; }
    public string? GstNumber { get; set; }
    public string? Address { get; set; }
    public decimal OutstandingPayable { get; set; }
    public bool IsActive { get; set; }
}
