namespace VGS.RetailOS.Modules.CustomerManagement.Customer.BO;

public class CustomerBO
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string? LastName { get; set; }
    public string Mobile { get; set; } = null!;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public decimal CreditBalance { get; set; }
    public bool IsActive { get; set; }
}
