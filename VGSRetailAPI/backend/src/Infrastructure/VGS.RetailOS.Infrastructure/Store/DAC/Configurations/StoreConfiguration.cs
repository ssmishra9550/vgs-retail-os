using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VGS.RetailOS.Infrastructure.Organization.DAC.Entities;
using VGS.RetailOS.Infrastructure.Store.DAC.Entities;

namespace VGS.RetailOS.Infrastructure.Store.DAC.Configurations;

public class StoreConfiguration : IEntityTypeConfiguration<StoreEntity>
{
    public void Configure(EntityTypeBuilder<StoreEntity> builder)
    {
        builder.ToTable("Stores");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Code)
            .HasMaxLength(50);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.ContactEmail)
            .HasMaxLength(256);

        builder.Property(x => x.ContactPhone)
            .HasMaxLength(50);

        builder.HasIndex(x => new { x.TenantId, x.OrganizationId, x.Name })
            .IsUnique();

        if (builder.Metadata.Model.FindEntityType(typeof(OrganizationEntity)) != null)
        {
            builder.HasOne<OrganizationEntity>()
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
