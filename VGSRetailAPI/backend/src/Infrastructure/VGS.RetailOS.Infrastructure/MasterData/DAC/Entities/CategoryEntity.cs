using VGS.RetailOS.Shared.Audit;

namespace VGS.RetailOS.Infrastructure.MasterData.DAC.Entities;

public class CategoryEntity : IAuditableEntity
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
