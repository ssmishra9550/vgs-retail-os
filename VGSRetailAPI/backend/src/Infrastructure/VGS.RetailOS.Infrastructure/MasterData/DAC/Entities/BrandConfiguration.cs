using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VGS.RetailOS.Infrastructure.MasterData.DAC.Entities;

public class BrandConfiguration : IEntityTypeConfiguration<BrandEntity>
{
    public void Configure(EntityTypeBuilder<BrandEntity> builder)
    {
        builder.ToTable("Brands");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.Description)
            .HasMaxLength(512);

        builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
    }
}
