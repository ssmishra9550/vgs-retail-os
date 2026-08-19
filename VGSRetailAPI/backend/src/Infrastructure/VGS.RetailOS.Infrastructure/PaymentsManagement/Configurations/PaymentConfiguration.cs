using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VGS.RetailOS.Infrastructure.PaymentsManagement.Entities;

namespace VGS.RetailOS.Infrastructure.PaymentsManagement.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<PaymentEntity>
{
    public void Configure(EntityTypeBuilder<PaymentEntity> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).IsRequired().HasMaxLength(64);
        builder.Property(x => x.PaymentType).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Amount).HasColumnType("decimal(18,4)");
        builder.Property(x => x.PaymentMethod).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ReferenceNumber).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(500);

        builder.HasIndex(x => new { x.TenantId, x.StoreId, x.ReferenceId });
    }
}
