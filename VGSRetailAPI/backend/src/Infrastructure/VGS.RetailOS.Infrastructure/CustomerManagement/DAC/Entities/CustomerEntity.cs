using VGS.RetailOS.Shared.Audit;

namespace VGS.RetailOS.Infrastructure.CustomerManagement.DAC.Entities;

public class CustomerEntity : IAuditableEntity, ISoftDeletable
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string? LastName { get; set; }
    public string Mobile { get; set; } = null!;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public decimal CreditBalance { get; set; }
    public bool IsActive { get; set; } = true;
    
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
