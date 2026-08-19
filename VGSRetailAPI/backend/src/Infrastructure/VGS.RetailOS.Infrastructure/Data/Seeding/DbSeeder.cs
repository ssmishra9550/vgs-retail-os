using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VGS.RetailOS.Infrastructure.Auth.DAC.Entities;
using VGS.RetailOS.Infrastructure.Organization.DAC.Entities;
using VGS.RetailOS.Infrastructure.Store.DAC.Entities;
using VGS.RetailOS.Infrastructure.MasterData.DAC.Entities;
using VGS.RetailOS.Infrastructure.ProductManagement.DAC.Entities;
using VGS.RetailOS.Infrastructure.InventoryManagement.DAC.Entities;
using VGS.RetailOS.Infrastructure.CustomerManagement.DAC.Entities;

namespace VGS.RetailOS.Infrastructure.Data.Seeding;

public static class DbSeeder
{
    public static async Task SeedDatabaseAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        try
        {
            logger.LogInformation("Ensuring database is created...");
            await context.Database.EnsureCreatedAsync();

            if (await context.Organizations.AnyAsync())
            {
                logger.LogInformation("Database already seeded.");
                return;
            }

            logger.LogInformation("Seeding initial data...");

            // 1. Create Organization (Tenant)
            var orgId = Guid.NewGuid();
            var tenantId = "vgs-tenant-01";
            var org = new OrganizationEntity
            {
                Id = orgId,
                TenantId = tenantId,
                Name = "VGS Retail Group",
                ContactEmail = "admin@vgs.com",
                ContactPhone = "1234567890",
                CreatedAt = DateTimeOffset.UtcNow
            };
            context.Organizations.Add(org);

            // 2. Create Roles
            var adminRole = new ApplicationRole { Name = "Admin", NormalizedName = "ADMIN", TenantId = tenantId };
            var cashierRole = new ApplicationRole { Name = "Cashier", NormalizedName = "CASHIER", TenantId = tenantId };
            await roleManager.CreateAsync(adminRole);
            await roleManager.CreateAsync(cashierRole);

            // 3. Create User
            var adminUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "admin@vgs.com",
                Email = "admin@vgs.com",
                FirstName = "System",
                LastName = "Admin",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await userManager.CreateAsync(adminUser, "P@ssw0rd123!");
            await userManager.AddToRoleAsync(adminUser, "Admin");

            // 4. Create Stores
            var store1Id = Guid.NewGuid();
            var store1 = new StoreEntity
            {
                Id = store1Id,
                TenantId = tenantId,
                OrganizationId = orgId,
                Name = "VGS Downtown",
                Code = "VGS-01",
                Address = "123 Main St",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };
            
            var store2Id = Guid.NewGuid();
            var store2 = new StoreEntity
            {
                Id = store2Id,
                TenantId = tenantId,
                OrganizationId = orgId,
                Name = "VGS Uptown",
                Code = "VGS-02",
                Address = "456 North Ave",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };
            context.Stores.AddRange(store1, store2);

            // 5. Create Master Data
            var catId = Guid.NewGuid();
            var category = new CategoryEntity { Id = catId, TenantId = tenantId, Name = "Electronics", IsActive = true, CreatedAt = DateTimeOffset.UtcNow };
            
            var brandId = Guid.NewGuid();
            var brand = new BrandEntity { Id = brandId, TenantId = tenantId, Name = "Sony", IsActive = true, CreatedAt = DateTimeOffset.UtcNow };

            var unitId = Guid.NewGuid();
            var unit = new UnitEntity { Id = unitId, TenantId = tenantId, Name = "Piece", IsActive = true, CreatedAt = DateTimeOffset.UtcNow };

            var taxId = Guid.NewGuid();
            var tax = new TaxEntity { Id = taxId, TenantId = tenantId, Name = "Standard VAT", Rate = 18.0m, IsActive = true, CreatedAt = DateTimeOffset.UtcNow };

            context.Categories.Add(category);
            context.Brands.Add(brand);
            context.Units.Add(unit);
            context.Taxes.Add(tax);

            // 6. Create Products
            var prod1Id = Guid.NewGuid();
            var prod1 = new ProductEntity
            {
                Id = prod1Id,
                TenantId = tenantId,
                Name = "Sony PlayStation 5",
                Sku = "SKU-PS5-001",
                Description = "Next Gen Console",
                CategoryId = catId,
                BrandId = brandId,
                UnitId = unitId,
                TaxId = taxId,
                PurchasePrice = 400.00m,
                SellingPrice = 499.99m,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };
            context.Products.Add(prod1);

            // 7. Create Customers
            var cust1Id = Guid.NewGuid();
            var customer = new CustomerEntity
            {
                Id = cust1Id,
                TenantId = tenantId,
                FirstName = "John",
                LastName = "Doe",
                Mobile = "555-1234",
                Email = "john@example.com",
                CreditBalance = 1000m,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };
            context.Customers.Add(customer);

            // 8. Add Initial Stock
            var stock = new StockBalanceEntity
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                StoreId = store1Id,
                ProductId = prod1Id,
                Quantity = 50,
                LastUpdated = DateTimeOffset.UtcNow
            };
            context.StockBalances.Add(stock);

            await context.SaveChangesAsync();

            logger.LogInformation("Database seeded successfully!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }
}
