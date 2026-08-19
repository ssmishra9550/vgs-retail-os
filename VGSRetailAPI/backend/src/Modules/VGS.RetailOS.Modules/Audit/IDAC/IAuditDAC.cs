using VGS.RetailOS.Contracts.V1.Audit.Requests;
using VGS.RetailOS.Modules.Audit.BO;
using VGS.RetailOS.Shared.BuildingBlocks.Pagination;

namespace VGS.RetailOS.Modules.Audit.IDAC;

public interface IAuditDAC
{
    Task<PaginatedList<AuditLogBO>> GetAuditLogsAsync(GetAuditLogsRequest request, string tenantId, CancellationToken cancellationToken);
    Task CreateAuditLogAsync(AuditLogBO auditLog, CancellationToken cancellationToken);
}
