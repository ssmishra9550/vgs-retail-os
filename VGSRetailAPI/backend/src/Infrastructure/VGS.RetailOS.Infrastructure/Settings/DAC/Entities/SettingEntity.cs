using VGS.RetailOS.Shared.Audit;

namespace VGS.RetailOS.Infrastructure.Settings.DAC.Entities;

public class SettingEntity : IAuditableEntity
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = null!;
    public Guid? StoreId { get; set; }
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string Group { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
