using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VGS.RetailOS.Infrastructure.Audit.DAC.Entities;
using VGS.RetailOS.Shared.Audit;
using VGS.RetailOS.Shared.Auth;
using VGS.RetailOS.Shared.Tenancy;

namespace VGS.RetailOS.Infrastructure.Data.Interceptors;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ITenantContextAccessor _tenantContextAccessor;
    private readonly IUserContextAccessor _userContextAccessor;

    public AuditSaveChangesInterceptor(ITenantContextAccessor tenantContextAccessor, IUserContextAccessor userContextAccessor)
    {
        _tenantContextAccessor = tenantContextAccessor;
        _userContextAccessor = userContextAccessor;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            ProcessAuditableEntities(eventData.Context);
        }
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            ProcessAuditableEntities(eventData.Context);
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ProcessAuditableEntities(DbContext context)
    {
        var tenantId = _tenantContextAccessor.TenantContext?.CurrentTenantId;
        var userId = _userContextAccessor.CurrentUserId;
        var now = DateTimeOffset.UtcNow;

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
            .ToList();

        var auditLogs = new List<AuditLogEntity>();

        foreach (var entry in entries)
        {
            if (entry.Entity is IAuditableEntity auditableEntity)
            {
                if (entry.State == EntityState.Added)
                {
                    auditableEntity.CreatedAt = now;
                    auditableEntity.CreatedBy = userId;
                }
                else if (entry.State == EntityState.Modified)
                {
                    auditableEntity.UpdatedAt = now;
                    auditableEntity.UpdatedBy = userId;
                }
            }

            // Generate Audit Log
            // Wait, we can't reliably get the primary key for Added entities BEFORE SaveChanges using this basic approach, 
            // but for a foundational phase this is acceptable, or we can use a two-phase commit approach if we really need PKs for inserts.
            // For now, let's capture the state.
            
            if (string.IsNullOrEmpty(tenantId))
            {
                // Skip auditing if there is no tenant context
                continue;
            }

            var entityType = entry.Metadata.Name;
            var primaryKey = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
            var entityId = primaryKey?.CurrentValue?.ToString() ?? "Unknown";

            var auditLog = new AuditLogEntity
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = userId,
                Action = entry.State.ToString(),
                EntityType = entityType,
                EntityId = entityId,
                Timestamp = now
            };

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary) continue; // Skip temporary properties (e.g. auto-generated IDs)

                string propertyName = property.Metadata.Name;

                switch (entry.State)
                {
                    case EntityState.Added:
                        newValues[propertyName] = property.CurrentValue;
                        break;
                    case EntityState.Deleted:
                        oldValues[propertyName] = property.OriginalValue;
                        break;
                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            oldValues[propertyName] = property.OriginalValue;
                            newValues[propertyName] = property.CurrentValue;
                        }
                        break;
                }
            }

            if (oldValues.Count > 0)
                auditLog.OldValues = JsonSerializer.Serialize(oldValues);

            if (newValues.Count > 0)
                auditLog.NewValues = JsonSerializer.Serialize(newValues);

            auditLogs.Add(auditLog);
        }

        if (auditLogs.Count > 0)
        {
            context.Set<AuditLogEntity>().AddRange(auditLogs);
        }
    }
}
