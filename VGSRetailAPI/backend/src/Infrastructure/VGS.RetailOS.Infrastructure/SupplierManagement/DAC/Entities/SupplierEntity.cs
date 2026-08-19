using VGS.RetailOS.Shared.Audit;

namespace VGS.RetailOS.Infrastructure.SupplierManagement.DAC.Entities;

public class SupplierEntity : IAuditableEntity
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? ContactPerson { get; set; }
    public string Mobile { get; set; } = null!;
    public string? Email { get; set; }
    public string? GstNumber { get; set; }
    public string? Address { get; set; }
    public decimal OutstandingPayable { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
