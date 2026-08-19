namespace VGS.RetailOS.Contracts.V1.Audit.Requests;

public class GetAuditLogsRequest
{
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public Guid? UserId { get; set; }
    public string? Action { get; set; }
    
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
