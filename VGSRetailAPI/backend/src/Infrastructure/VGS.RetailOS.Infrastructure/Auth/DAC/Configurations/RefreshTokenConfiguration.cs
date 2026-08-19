using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VGS.RetailOS.Infrastructure.Auth.DAC.Entities;

namespace VGS.RetailOS.Infrastructure.Auth.DAC.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshTokenEntity>
{
    public void Configure(EntityTypeBuilder<RefreshTokenEntity> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.TokenHash)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(r => r.CreatedFromIp)
            .HasMaxLength(64);

        builder.Property(r => r.UserAgent)
            .HasMaxLength(512);

        builder.Property(r => r.RevocationReason)
            .HasMaxLength(256);

        builder.HasIndex(r => r.TokenHash)
            .IsUnique();

        builder.HasIndex(r => r.UserId);

        builder.HasIndex(r => r.FamilyId);

        builder.HasIndex(r => new { r.UserId, r.IsRevoked, r.ExpiresAt });

        builder.HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
