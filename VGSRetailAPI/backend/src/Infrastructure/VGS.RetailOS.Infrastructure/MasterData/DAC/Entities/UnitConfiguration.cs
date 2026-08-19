using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VGS.RetailOS.Infrastructure.MasterData.DAC.Entities;

public class UnitConfiguration : IEntityTypeConfiguration<UnitEntity>
{
    public void Configure(EntityTypeBuilder<UnitEntity> builder)
    {
        builder.ToTable("Units");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.ShortName)
            .IsRequired()
            .HasMaxLength(16);

        builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.ShortName }).IsUnique();
    }
}
