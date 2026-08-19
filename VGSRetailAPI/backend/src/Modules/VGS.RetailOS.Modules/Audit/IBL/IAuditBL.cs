using VGS.RetailOS.Contracts.V1.Audit.Requests;
using VGS.RetailOS.Contracts.V1.Audit.Responses;
using VGS.RetailOS.Shared.BuildingBlocks.Pagination;

namespace VGS.RetailOS.Modules.Audit.IBL;

public interface IAuditBL
{
    Task<PaginatedList<AuditLogResponse>> GetAuditLogsAsync(GetAuditLogsRequest request, CancellationToken cancellationToken);
    Task LogBusinessEventAsync(string action, string entityType, string entityId, string? reason, string? oldValues, string? newValues, string? correlationId, CancellationToken cancellationToken);
}
