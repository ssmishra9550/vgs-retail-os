using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VGS.RetailOS.Infrastructure.Settings.DAC.Entities;

namespace VGS.RetailOS.Infrastructure.Settings.DAC.Configurations;

public class SettingConfiguration : IEntityTypeConfiguration<SettingEntity>
{
    public void Configure(EntityTypeBuilder<SettingEntity> builder)
    {
        builder.ToTable("Settings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.Group)
            .IsRequired()
            .HasMaxLength(100);

        // A setting key must be unique per tenant and store scope
        builder.HasIndex(x => new { x.TenantId, x.StoreId, x.Key })
            .IsUnique();
    }
}
