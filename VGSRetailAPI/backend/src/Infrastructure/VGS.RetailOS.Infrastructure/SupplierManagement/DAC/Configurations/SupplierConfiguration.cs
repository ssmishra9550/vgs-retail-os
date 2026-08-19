using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VGS.RetailOS.Infrastructure.SupplierManagement.DAC.Entities;

namespace VGS.RetailOS.Infrastructure.SupplierManagement.DAC.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<SupplierEntity>
{
    public void Configure(EntityTypeBuilder<SupplierEntity> builder)
    {
        builder.ToTable("Suppliers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ContactPerson)
            .HasMaxLength(100);

        builder.Property(x => x.Mobile)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Email)
            .HasMaxLength(100);

        builder.Property(x => x.GstNumber)
            .HasMaxLength(50);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.OutstandingPayable)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        // A tenant cannot have two suppliers with the same name
        builder.HasIndex(x => new { x.TenantId, x.Name })
            .IsUnique();

        // A tenant cannot have two suppliers with the same mobile
        builder.HasIndex(x => new { x.TenantId, x.Mobile })
            .IsUnique();
    }
}
