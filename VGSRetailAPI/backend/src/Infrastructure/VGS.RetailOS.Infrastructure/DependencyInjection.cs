using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using VGS.RetailOS.Infrastructure.Caching;
using VGS.RetailOS.Infrastructure.Data;
using VGS.RetailOS.Infrastructure.HealthChecks;
using VGS.RetailOS.Shared.BuildingBlocks.Caching;

namespace VGS.RetailOS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddScoped<VGS.RetailOS.Shared.Auth.IUserContextAccessor, VGS.RetailOS.Shared.Auth.UserContextAccessor>();
        services.AddScoped<VGS.RetailOS.Infrastructure.Data.Interceptors.AuditSaveChangesInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<VGS.RetailOS.Infrastructure.Data.Interceptors.AuditSaveChangesInterceptor>();
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
                   .AddInterceptors(interceptor);
        });

        services.AddIdentityCore<VGS.RetailOS.Infrastructure.Auth.DAC.Entities.ApplicationUser>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 12;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<VGS.RetailOS.Infrastructure.Auth.DAC.Entities.ApplicationRole>()
        .AddEntityFrameworkStores<AppDbContext>();

        services.AddTransient<Microsoft.AspNetCore.Identity.IPasswordHasher<VGS.RetailOS.Infrastructure.Auth.DAC.Entities.ApplicationUser>, Microsoft.AspNetCore.Identity.PasswordHasher<VGS.RetailOS.Infrastructure.Auth.DAC.Entities.ApplicationUser>>();
        services.AddTransient<VGS.RetailOS.Modules.Auth.IBL.IPasswordVerifier, VGS.RetailOS.Infrastructure.Auth.DAC.IdentityPasswordVerifier>();
        services.AddScoped<VGS.RetailOS.Modules.Auth.IDAC.IAuthDAC, VGS.RetailOS.Infrastructure.Auth.DAC.AuthDAC>();
        services.AddScoped<VGS.RetailOS.Modules.Organization.IDAC.IOrganizationDAC, VGS.RetailOS.Infrastructure.Organization.DAC.OrganizationDAC>();
        services.AddScoped<VGS.RetailOS.Modules.Store.IDAC.IStoreDAC, VGS.RetailOS.Infrastructure.Store.DAC.StoreDAC>();
        services.AddScoped<VGS.RetailOS.Modules.User.IDAC.IUserDAC, VGS.RetailOS.Infrastructure.User.DAC.UserDAC>();
        services.AddScoped<VGS.RetailOS.Modules.Role.IDAC.IRoleDAC, VGS.RetailOS.Infrastructure.Role.DAC.RoleDAC>();
        services.AddScoped<VGS.RetailOS.Modules.Audit.IDAC.IAuditDAC, VGS.RetailOS.Infrastructure.Audit.DAC.AuditDAC>();
        services.AddScoped<VGS.RetailOS.Modules.MasterData.Category.IDAC.ICategoryDAC, VGS.RetailOS.Infrastructure.MasterData.DAC.CategoryDAC>();
        services.AddScoped<VGS.RetailOS.Modules.MasterData.Brand.IDAC.IBrandDAC, VGS.RetailOS.Infrastructure.MasterData.DAC.BrandDAC>();
        services.AddScoped<VGS.RetailOS.Modules.MasterData.Unit.IDAC.IUnitDAC, VGS.RetailOS.Infrastructure.MasterData.DAC.UnitDAC>();
        services.AddScoped<VGS.RetailOS.Modules.MasterData.Tax.IDAC.ITaxDAC, VGS.RetailOS.Infrastructure.MasterData.DAC.TaxDAC>();
        services.AddScoped<VGS.RetailOS.Modules.ProductManagement.Product.IDAC.IProductDAC, VGS.RetailOS.Infrastructure.ProductManagement.DAC.ProductDAC>();
        services.AddScoped<VGS.RetailOS.Modules.CustomerManagement.Customer.IDAC.ICustomerDAC, VGS.RetailOS.Infrastructure.CustomerManagement.DAC.CustomerDAC>();
        services.AddScoped<VGS.RetailOS.Modules.Settings.Setting.IDAC.ISettingDAC, VGS.RetailOS.Infrastructure.Settings.DAC.SettingDAC>();
        services.AddScoped<VGS.RetailOS.Modules.SupplierManagement.Supplier.IDAC.ISupplierDAC, VGS.RetailOS.Infrastructure.SupplierManagement.DAC.SupplierDAC>();
        
        // Inventory Management
        services.AddScoped<VGS.RetailOS.Modules.InventoryManagement.Inventory.IDAC.IInventoryDAC, VGS.RetailOS.Infrastructure.InventoryManagement.DAC.InventoryDAC>();
        services.AddScoped<VGS.RetailOS.Modules.InventoryManagement.Inventory.IBL.IInventoryBL, VGS.RetailOS.Modules.InventoryManagement.Inventory.BL.InventoryBL>();
        
        // Purchasing Management
        services.AddScoped<VGS.RetailOS.Modules.PurchasingManagement.Purchase.IDAC.IPurchaseDAC, VGS.RetailOS.Infrastructure.PurchasingManagement.DAC.PurchaseDAC>();
        services.AddScoped<VGS.RetailOS.Modules.PurchasingManagement.Purchase.IBL.IPurchaseBL, VGS.RetailOS.Modules.PurchasingManagement.Purchase.BL.PurchaseBL>();
        
        // Sales Management
        services.AddScoped<VGS.RetailOS.Modules.SalesManagement.Sale.IDAC.ISaleDAC, VGS.RetailOS.Infrastructure.SalesManagement.DAC.SaleDAC>();
        services.AddScoped<VGS.RetailOS.Modules.SalesManagement.Sale.IBL.ISaleBL, VGS.RetailOS.Modules.SalesManagement.Sale.BL.SaleBL>();

        // Payments Management
        services.AddScoped<VGS.RetailOS.Modules.PaymentsManagement.Payment.IDAC.IPaymentDAC, VGS.RetailOS.Infrastructure.PaymentsManagement.DAC.PaymentDAC>();
        services.AddScoped<VGS.RetailOS.Modules.PaymentsManagement.Payment.IBL.IPaymentBL, VGS.RetailOS.Modules.PaymentsManagement.Payment.BL.PaymentBL>();

        // Expenses Management
        services.AddScoped<VGS.RetailOS.Modules.ExpensesManagement.Expense.IDAC.IExpenseDAC, VGS.RetailOS.Infrastructure.ExpensesManagement.DAC.ExpenseDAC>();
        services.AddScoped<VGS.RetailOS.Modules.ExpensesManagement.Expense.IBL.IExpenseBL, VGS.RetailOS.Modules.ExpensesManagement.Expense.BL.ExpenseBL>();
        
        // Report Management
        services.AddScoped<VGS.RetailOS.Modules.ReportsManagement.Report.IDAC.IReportDAC, VGS.RetailOS.Infrastructure.ReportsManagement.DAC.ReportDAC>();
        services.AddScoped<VGS.RetailOS.Modules.ReportsManagement.Report.IBL.IReportBL, VGS.RetailOS.Modules.ReportsManagement.Report.BL.ReportBL>();

        var jwtSection = configuration.GetSection(VGS.RetailOS.Infrastructure.Auth.Tokens.JwtOptions.SectionName);
        services.Configure<VGS.RetailOS.Infrastructure.Auth.Tokens.JwtOptions>(jwtSection);
        services.AddTransient<VGS.RetailOS.Modules.Auth.IBL.ITokenService, VGS.RetailOS.Infrastructure.Auth.Tokens.TokenService>();

        var jwtOptions = jwtSection.Get<VGS.RetailOS.Infrastructure.Auth.Tokens.JwtOptions>() ?? new VGS.RetailOS.Infrastructure.Auth.Tokens.JwtOptions();
        jwtOptions.Validate();

        var signingKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtOptions.SecretKey));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ClockSkew = TimeSpan.FromSeconds(5)
            };
        });

        services.AddAuthorization();

        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (string.IsNullOrEmpty(redisConnectionString))
        {
            throw new InvalidOperationException("Redis connection string is missing.");
        }

        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisConnectionString));

        services.AddTransient<IRedisCache, RedisCacheService>();

        services.AddHealthChecks()
            .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: new[] { "live" })
            .AddDbContextCheck<AppDbContext>("postgresql", tags: new[] { "ready", "db" })
            .AddCheck<RedisHealthCheck>("redis", tags: new[] { "ready", "cache" });

        return services;
    }
}
