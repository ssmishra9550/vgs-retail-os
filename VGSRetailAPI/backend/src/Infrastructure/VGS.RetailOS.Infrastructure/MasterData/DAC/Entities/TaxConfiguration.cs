using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VGS.RetailOS.Infrastructure.MasterData.DAC.Entities;

public class TaxConfiguration : IEntityTypeConfiguration<TaxEntity>
{
    public void Configure(EntityTypeBuilder<TaxEntity> builder)
    {
        builder.ToTable("Taxes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.Rate)
            .HasColumnType("decimal(18,4)");

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
    }
}
