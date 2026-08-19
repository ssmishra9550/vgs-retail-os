using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VGS.RetailOS.Infrastructure.ExpensesManagement.Entities;

namespace VGS.RetailOS.Infrastructure.ExpensesManagement.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<ExpenseEntity>
{
    public void Configure(EntityTypeBuilder<ExpenseEntity> builder)
    {
        builder.ToTable("Expenses");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).IsRequired().HasMaxLength(64);
        builder.Property(x => x.Category).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Amount).HasColumnType("decimal(18,4)");
        builder.Property(x => x.PaymentMethod).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Status).IsRequired().HasMaxLength(50);

        builder.HasIndex(x => new { x.TenantId, x.StoreId, x.ExpenseDate });
    }
}
