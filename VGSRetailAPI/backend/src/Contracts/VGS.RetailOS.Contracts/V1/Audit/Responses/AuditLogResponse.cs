namespace VGS.RetailOS.Contracts.V1.Audit.Responses;

public class AuditLogResponse
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public string EntityId { get; set; } = null!;
    public DateTimeOffset Timestamp { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? Reason { get; set; }
    public string? CorrelationId { get; set; }
}
