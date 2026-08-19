using Microsoft.EntityFrameworkCore;
using VGS.RetailOS.Contracts.V1.Audit.Requests;
using VGS.RetailOS.Infrastructure.Audit.DAC.Entities;
using VGS.RetailOS.Infrastructure.Data;
using VGS.RetailOS.Modules.Audit.BO;
using VGS.RetailOS.Modules.Audit.IDAC;
using VGS.RetailOS.Shared.BuildingBlocks.Pagination;

namespace VGS.RetailOS.Infrastructure.Audit.DAC;

public class AuditDAC : IAuditDAC
{
    private readonly AppDbContext _dbContext;

    public AuditDAC(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginatedList<AuditLogBO>> GetAuditLogsAsync(GetAuditLogsRequest request, string tenantId, CancellationToken cancellationToken)
    {
        var query = _dbContext.AuditLogs.AsNoTracking().Where(a => a.TenantId == tenantId);

        if (!string.IsNullOrEmpty(request.EntityType))
            query = query.Where(a => a.EntityType == request.EntityType);
            
        if (!string.IsNullOrEmpty(request.EntityId))
            query = query.Where(a => a.EntityId == request.EntityId);
            
        if (!string.IsNullOrEmpty(request.Action))
            query = query.Where(a => a.Action == request.Action);
            
        if (request.UserId.HasValue)
            query = query.Where(a => a.UserId == request.UserId.Value);
            
        if (request.StartDate.HasValue)
            query = query.Where(a => a.Timestamp >= request.StartDate.Value);
            
        if (request.EndDate.HasValue)
            query = query.Where(a => a.Timestamp <= request.EndDate.Value);

        var count = await query.CountAsync(cancellationToken);
        
        var items = await query.OrderByDescending(a => a.Timestamp)
                               .Skip((request.PageNumber - 1) * request.PageSize)
                               .Take(request.PageSize)
                               .Select(a => new AuditLogBO
                               {
                                   Id = a.Id,
                                   TenantId = a.TenantId,
                                   UserId = a.UserId,
                                   Action = a.Action,
                                   EntityType = a.EntityType,
                                   EntityId = a.EntityId,
                                   Timestamp = a.Timestamp,
                                   OldValues = a.OldValues,
                                   NewValues = a.NewValues,
                                   Reason = a.Reason,
                                   CorrelationId = a.CorrelationId
                               })
                               .ToListAsync(cancellationToken);

        return new PaginatedList<AuditLogBO>(items, count, request.PageNumber, request.PageSize);
    }

    public async Task CreateAuditLogAsync(AuditLogBO auditLog, CancellationToken cancellationToken)
    {
        var entity = new AuditLogEntity
        {
            Id = auditLog.Id,
            TenantId = auditLog.TenantId,
            UserId = auditLog.UserId,
            Action = auditLog.Action,
            EntityType = auditLog.EntityType,
            EntityId = auditLog.EntityId,
            Timestamp = auditLog.Timestamp,
            OldValues = auditLog.OldValues,
            NewValues = auditLog.NewValues,
            Reason = auditLog.Reason,
            CorrelationId = auditLog.CorrelationId
        };

        _dbContext.AuditLogs.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
