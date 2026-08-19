using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VGS.RetailOS.Infrastructure.CustomerManagement.DAC.Entities;

namespace VGS.RetailOS.Infrastructure.CustomerManagement.DAC.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<CustomerEntity>
{
    public void Configure(EntityTypeBuilder<CustomerEntity> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.LastName)
            .HasMaxLength(100);

        builder.Property(x => x.Mobile)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Email)
            .HasMaxLength(100);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.CreditBalance)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        // A tenant cannot have two active customers with the exact same Mobile number
        builder.HasIndex(x => new { x.TenantId, x.Mobile })
            .IsUnique();
    }
}
