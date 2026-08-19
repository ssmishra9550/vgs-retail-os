using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VGS.RetailOS.Infrastructure.User.DAC.Entities;

namespace VGS.RetailOS.Infrastructure.User.DAC.Configurations;

public class TenantUserMembershipConfiguration : IEntityTypeConfiguration<TenantUserMembershipEntity>
{
    public void Configure(EntityTypeBuilder<TenantUserMembershipEntity> builder)
    {
        builder.ToTable("TenantUserMemberships");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => new { x.UserId, x.TenantId })
            .IsUnique();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
