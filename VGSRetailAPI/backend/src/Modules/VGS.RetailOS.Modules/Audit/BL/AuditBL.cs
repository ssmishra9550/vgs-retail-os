using VGS.RetailOS.Contracts.V1.Audit.Requests;
using VGS.RetailOS.Contracts.V1.Audit.Responses;
using VGS.RetailOS.Modules.Audit.BO;
using VGS.RetailOS.Modules.Audit.IBL;
using VGS.RetailOS.Modules.Audit.IDAC;
using VGS.RetailOS.Shared.Auth;
using VGS.RetailOS.Shared.BuildingBlocks.Pagination;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;

namespace VGS.RetailOS.Modules.Audit.BL;

public class AuditBL : IAuditBL
{
    private readonly IAuditDAC _auditDac;
    private readonly ITenantContextAccessor _tenantContextAccessor;
    private readonly IUserContextAccessor _userContextAccessor;

    public AuditBL(IAuditDAC auditDac, ITenantContextAccessor tenantContextAccessor, IUserContextAccessor userContextAccessor)
    {
        _auditDac = auditDac;
        _tenantContextAccessor = tenantContextAccessor;
        _userContextAccessor = userContextAccessor;
    }

    public async Task<PaginatedList<AuditLogResponse>> GetAuditLogsAsync(GetAuditLogsRequest request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextAccessor.TenantContext?.CurrentTenantId;
        if (string.IsNullOrEmpty(tenantId))
        {
            throw new UnauthorizedException("Tenant context is missing.");
        }

        var result = await _auditDac.GetAuditLogsAsync(request, tenantId, cancellationToken);

        var responses = result.Items.Select(a => new AuditLogResponse
        {
            Id = a.Id,
            UserId = a.UserId,
            Action = a.Action,
            EntityType = a.EntityType,
            EntityId = a.EntityId,
            Timestamp = a.Timestamp,
            OldValues = a.OldValues,
            NewValues = a.NewValues,
            Reason = a.Reason,
            CorrelationId = a.CorrelationId
        }).ToList();

        return new PaginatedList<AuditLogResponse>(responses, result.TotalCount, result.PageNumber, result.PageSize);
    }

    public async Task LogBusinessEventAsync(string action, string entityType, string entityId, string? reason, string? oldValues, string? newValues, string? correlationId, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextAccessor.TenantContext?.CurrentTenantId;
        if (string.IsNullOrEmpty(tenantId))
        {
            return; // Or throw depending on requirements
        }

        var userId = _userContextAccessor.CurrentUserId;

        var auditLog = new AuditLogBO
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Timestamp = DateTimeOffset.UtcNow,
            OldValues = oldValues,
            NewValues = newValues,
            Reason = reason,
            CorrelationId = correlationId
        };

        await _auditDac.CreateAuditLogAsync(auditLog, cancellationToken);
    }
}
