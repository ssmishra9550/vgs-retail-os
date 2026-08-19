using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VGS.RetailOS.Infrastructure.Auth.DAC.Entities;
using VGS.RetailOS.Shared.Tenancy;

namespace VGS.RetailOS.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private readonly ITenantContextAccessor? _tenantContextAccessor;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContextAccessor? tenantContextAccessor = null) : base(options)
    {
        _tenantContextAccessor = tenantContextAccessor;
    }

    public DbSet<RefreshTokenEntity> RefreshTokens { get; set; } = default!;
    public DbSet<VGS.RetailOS.Infrastructure.Organization.DAC.Entities.OrganizationEntity> Organizations { get; set; } = default!;
    public DbSet<VGS.RetailOS.Infrastructure.Store.DAC.Entities.StoreEntity> Stores { get; set; } = default!;
    public DbSet<VGS.RetailOS.Infrastructure.User.DAC.Entities.TenantUserMembershipEntity> TenantUserMemberships { get; set; } = default!;

    public DbSet<VGS.RetailOS.Infrastructure.Audit.DAC.Entities.AuditLogEntity> AuditLogs { get; set; } = default!;
    public DbSet<VGS.RetailOS.Infrastructure.MasterData.DAC.Entities.CategoryEntity> Categories { get; set; } = default!;
    public DbSet<VGS.RetailOS.Infrastructure.MasterData.DAC.Entities.BrandEntity> Brands { get; set; } = default!;
    public DbSet<VGS.RetailOS.Infrastructure.MasterData.DAC.Entities.UnitEntity> Units { get; set; } = default!;
    public DbSet<VGS.RetailOS.Infrastructure.MasterData.DAC.Entities.TaxEntity> Taxes { get; set; } = default!;
    public DbSet<VGS.RetailOS.Infrastructure.ProductManagement.DAC.Entities.ProductEntity> Products { get; set; } = default!;
    public DbSet<VGS.RetailOS.Infrastructure.CustomerManagement.DAC.Entities.CustomerEntity> Customers { get; set; } = default!;
    public DbSet<VGS.RetailOS.Infrastructure.Settings.DAC.Entities.SettingEntity> Settings { get; set; } = default!;
    public DbSet<VGS.RetailOS.Infrastructure.SupplierManagement.DAC.Entities.SupplierEntity> Suppliers { get; set; } = default!;
    public DbSet<VGS.RetailOS.Infrastructure.InventoryManagement.DAC.Entities.InventoryLedgerEntity> InventoryLedger { get; set; } = default!;
    public DbSet<VGS.RetailOS.Infrastructure.InventoryManagement.DAC.Entities.StockBalanceEntity> StockBalances { get; set; } = default!;
    public DbSet<VGS.RetailOS.Infrastructure.InventoryManagement.DAC.Entities.StockTransferEntity> StockTransfers { get; set; } = default!;
    public DbSet<VGS.RetailOS.Infrastructure.InventoryManagement.DAC.Entities.StockTransferItemEntity> StockTransferItems { get; set; } = default!;

    public DbSet<VGS.RetailOS.Infrastructure.PurchasingManagement.DAC.Entities.PurchaseEntity> Purchases { get; set; } = default!;
    public DbSet<VGS.RetailOS.Infrastructure.PurchasingManagement.DAC.Entities.PurchaseItemEntity> PurchaseItems { get; set; } = default!;
    public DbSet<VGS.RetailOS.Infrastructure.SalesManagement.DAC.Entities.SaleEntity> Sales { get; set; } = default!;
    public DbSet<VGS.RetailOS.Infrastructure.SalesManagement.DAC.Entities.SaleItemEntity> SaleItems { get; set; } = default!;
    public DbSet<VGS.RetailOS.Infrastructure.PaymentsManagement.Entities.PaymentEntity> Payments { get; set; } = default!;
    public DbSet<VGS.RetailOS.Infrastructure.ExpensesManagement.Entities.ExpenseEntity> Expenses { get; set; } = default!;
    public DbSet<VGS.RetailOS.Infrastructure.ReturnsManagement.DAC.Entities.ReturnEntity> Returns { get; set; } = default!;
    public DbSet<VGS.RetailOS.Infrastructure.ReturnsManagement.DAC.Entities.ReturnItemEntity> ReturnItems { get; set; } = default!;


    private string? CurrentTenantId => _tenantContextAccessor?.TenantContext?.CurrentTenantId;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Apply global query filters for tenant isolation
        builder.Entity<VGS.RetailOS.Infrastructure.Organization.DAC.Entities.OrganizationEntity>()
            .HasQueryFilter(o => CurrentTenantId == null || o.TenantId == CurrentTenantId);
            
        builder.Entity<VGS.RetailOS.Infrastructure.Store.DAC.Entities.StoreEntity>()
            .HasQueryFilter(s => CurrentTenantId == null || s.TenantId == CurrentTenantId);

        builder.Entity<VGS.RetailOS.Infrastructure.User.DAC.Entities.TenantUserMembershipEntity>()
            .HasQueryFilter(m => CurrentTenantId == null || m.TenantId == CurrentTenantId);

        builder.Entity<ApplicationRole>()
            .HasQueryFilter(r => CurrentTenantId == null || r.IsSystemRole || r.TenantId == CurrentTenantId);

        builder.Entity<VGS.RetailOS.Infrastructure.Audit.DAC.Entities.AuditLogEntity>()
            .HasQueryFilter(a => CurrentTenantId == null || a.TenantId == CurrentTenantId);

        builder.Entity<VGS.RetailOS.Infrastructure.MasterData.DAC.Entities.CategoryEntity>()
            .HasQueryFilter(c => CurrentTenantId == null || c.TenantId == CurrentTenantId);

        builder.Entity<VGS.RetailOS.Infrastructure.MasterData.DAC.Entities.BrandEntity>()
            .HasQueryFilter(b => CurrentTenantId == null || b.TenantId == CurrentTenantId);

        builder.Entity<VGS.RetailOS.Infrastructure.MasterData.DAC.Entities.UnitEntity>()
            .HasQueryFilter(u => CurrentTenantId == null || u.TenantId == CurrentTenantId);

        builder.Entity<VGS.RetailOS.Infrastructure.MasterData.DAC.Entities.TaxEntity>()
            .HasQueryFilter(t => CurrentTenantId == null || t.TenantId == CurrentTenantId);

        builder.Entity<VGS.RetailOS.Infrastructure.ProductManagement.DAC.Entities.ProductEntity>()
            .HasQueryFilter(p => (CurrentTenantId == null || p.TenantId == CurrentTenantId) && !p.IsDeleted);

        builder.Entity<VGS.RetailOS.Infrastructure.CustomerManagement.DAC.Entities.CustomerEntity>()
            .HasQueryFilter(c => (CurrentTenantId == null || c.TenantId == CurrentTenantId) && !c.IsDeleted);

        builder.Entity<VGS.RetailOS.Infrastructure.Settings.DAC.Entities.SettingEntity>()
            .HasQueryFilter(s => CurrentTenantId == null || s.TenantId == CurrentTenantId);

        builder.Entity<VGS.RetailOS.Infrastructure.SupplierManagement.DAC.Entities.SupplierEntity>()
            .HasQueryFilter(s => (CurrentTenantId == null || s.TenantId == CurrentTenantId) && !s.IsDeleted);

        builder.Entity<VGS.RetailOS.Infrastructure.InventoryManagement.DAC.Entities.InventoryLedgerEntity>()
            .HasQueryFilter(i => CurrentTenantId == null || i.TenantId == CurrentTenantId);

        builder.Entity<VGS.RetailOS.Infrastructure.InventoryManagement.DAC.Entities.StockBalanceEntity>()
            .HasQueryFilter(s => CurrentTenantId == null || s.TenantId == CurrentTenantId);

        builder.Entity<VGS.RetailOS.Infrastructure.InventoryManagement.DAC.Entities.StockTransferEntity>()
            .HasQueryFilter(s => CurrentTenantId == null || s.TenantId == CurrentTenantId);

        builder.Entity<VGS.RetailOS.Infrastructure.PurchasingManagement.DAC.Entities.PurchaseEntity>()
            .HasQueryFilter(p => CurrentTenantId == null || p.TenantId == CurrentTenantId);

        builder.Entity<VGS.RetailOS.Infrastructure.PurchasingManagement.DAC.Entities.PurchaseItemEntity>()
            .HasQueryFilter(p => CurrentTenantId == null || p.TenantId == CurrentTenantId);

        builder.Entity<VGS.RetailOS.Infrastructure.SalesManagement.DAC.Entities.SaleEntity>()
            .HasQueryFilter(s => CurrentTenantId == null || s.TenantId == CurrentTenantId);

        builder.Entity<VGS.RetailOS.Infrastructure.SalesManagement.DAC.Entities.SaleItemEntity>()
            .HasQueryFilter(s => CurrentTenantId == null || s.TenantId == CurrentTenantId);

        builder.Entity<VGS.RetailOS.Infrastructure.PaymentsManagement.Entities.PaymentEntity>()
            .HasQueryFilter(p => CurrentTenantId == null || p.TenantId == CurrentTenantId);

        builder.Entity<VGS.RetailOS.Infrastructure.ExpensesManagement.Entities.ExpenseEntity>()
            .HasQueryFilter(e => CurrentTenantId == null || e.TenantId == CurrentTenantId);

        builder.Entity<VGS.RetailOS.Infrastructure.ReturnsManagement.DAC.Entities.ReturnEntity>()
            .HasQueryFilter(r => CurrentTenantId == null || r.TenantId == CurrentTenantId);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<VGS.RetailOS.Shared.Audit.ISoftDeletable>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = DateTimeOffset.UtcNow;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
