using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VGS.RetailOS.Infrastructure.Audit.DAC.Entities;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLogEntity>
{
    public void Configure(EntityTypeBuilder<AuditLogEntity> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.Action)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.EntityType)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.EntityId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.Timestamp)
            .IsRequired();

        builder.Property(x => x.OldValues)
            .HasColumnType("jsonb");

        builder.Property(x => x.NewValues)
            .HasColumnType("jsonb");

        builder.Property(x => x.Reason)
            .HasMaxLength(512);

        builder.Property(x => x.CorrelationId)
            .HasMaxLength(128);

        builder.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId });
        builder.HasIndex(x => new { x.TenantId, x.Timestamp });
    }
}
