using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VGS.RetailOS.Infrastructure.Organization.DAC.Entities;
using VGS.RetailOS.Shared.Tenancy;

namespace VGS.RetailOS.Infrastructure.Organization.DAC.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<OrganizationEntity>
{
    public void Configure(EntityTypeBuilder<OrganizationEntity> builder)
    {
        builder.ToTable("Organizations");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.TenantId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.Code)
            .HasMaxLength(20);

        builder.Property(o => o.TaxId)
            .HasMaxLength(50);

        builder.Property(o => o.Address)
            .HasMaxLength(500);

        builder.Property(o => o.ContactEmail)
            .HasMaxLength(255);

        builder.Property(o => o.ContactPhone)
            .HasMaxLength(50);

        // Name must be unique within a tenant
        builder.HasIndex(o => new { o.TenantId, o.Name }).IsUnique();
    }
}
