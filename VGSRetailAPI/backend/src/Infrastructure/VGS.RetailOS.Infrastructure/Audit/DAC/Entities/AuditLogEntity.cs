namespace VGS.RetailOS.Infrastructure.Audit.DAC.Entities;

public class AuditLogEntity
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = null!;
    public Guid? UserId { get; set; }
    public string Action { get; set; } = null!; // e.g. "Create", "Update", "Delete", "BusinessEvent"
    public string EntityType { get; set; } = null!;
    public string EntityId { get; set; } = null!;
    public DateTimeOffset Timestamp { get; set; }
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public string? Reason { get; set; }
    public string? CorrelationId { get; set; }
}
