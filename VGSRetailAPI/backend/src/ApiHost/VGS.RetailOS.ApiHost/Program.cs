using VGS.RetailOS.Infrastructure;
using VGS.RetailOS.Shared.Errors;
using VGS.RetailOS.Shared.Observability.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddVgsStructuredLogging();
builder.Services.AddVgsErrorHandling();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<VGS.RetailOS.Modules.Auth.IBL.IAuthBL, VGS.RetailOS.Modules.Auth.BL.AuthBL>();
builder.Services.AddScoped<VGS.RetailOS.Modules.Organization.IBL.IOrganizationBL, VGS.RetailOS.Modules.Organization.BL.OrganizationBL>();
builder.Services.AddScoped<VGS.RetailOS.Modules.Store.IBL.IStoreBL, VGS.RetailOS.Modules.Store.BL.StoreBL>();
builder.Services.AddScoped<VGS.RetailOS.Modules.User.IBL.IUserBL, VGS.RetailOS.Modules.User.BL.UserBL>();
builder.Services.AddScoped<VGS.RetailOS.Modules.Role.IBL.IRoleBL, VGS.RetailOS.Modules.Role.BL.RoleBL>();
builder.Services.AddScoped<VGS.RetailOS.Modules.Audit.IBL.IAuditBL, VGS.RetailOS.Modules.Audit.BL.AuditBL>();
builder.Services.AddScoped<VGS.RetailOS.Modules.MasterData.Category.IBL.ICategoryBL, VGS.RetailOS.Modules.MasterData.Category.BL.CategoryBL>();
builder.Services.AddScoped<VGS.RetailOS.Modules.MasterData.Brand.IBL.IBrandBL, VGS.RetailOS.Modules.MasterData.Brand.BL.BrandBL>();
builder.Services.AddScoped<VGS.RetailOS.Modules.MasterData.Unit.IBL.IUnitBL, VGS.RetailOS.Modules.MasterData.Unit.BL.UnitBL>();
builder.Services.AddScoped<VGS.RetailOS.Modules.MasterData.Tax.IBL.ITaxBL, VGS.RetailOS.Modules.MasterData.Tax.BL.TaxBL>();
builder.Services.AddScoped<VGS.RetailOS.Modules.ProductManagement.Product.IBL.IProductBL, VGS.RetailOS.Modules.ProductManagement.Product.BL.ProductBL>();
builder.Services.AddScoped<VGS.RetailOS.Modules.CustomerManagement.Customer.IBL.ICustomerBL, VGS.RetailOS.Modules.CustomerManagement.Customer.BL.CustomerBL>();
builder.Services.AddScoped<VGS.RetailOS.Modules.Settings.Setting.IBL.ISettingBL, VGS.RetailOS.Modules.Settings.Setting.BL.SettingBL>();
builder.Services.AddScoped<VGS.RetailOS.Modules.SupplierManagement.Supplier.IBL.ISupplierBL, VGS.RetailOS.Modules.SupplierManagement.Supplier.BL.SupplierBL>();
builder.Services.AddScoped<VGS.RetailOS.Modules.InventoryManagement.Inventory.IBL.IInventoryBL, VGS.RetailOS.Modules.InventoryManagement.Inventory.BL.InventoryBL>();
builder.Services.AddSingleton<VGS.RetailOS.Shared.Tenancy.ITenantContextAccessor, VGS.RetailOS.Shared.Tenancy.TenantContextAccessor>();

var app = builder.Build();

app.UseVgsErrorHandling();
app.UseVgsRequestLogging();

app.UseCors("DefaultCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<VGS.RetailOS.ApiHost.Middleware.TenantResolutionMiddleware>();

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("live"),
    ResponseWriter = VGS.RetailOS.Shared.Observability.HealthChecks.HealthCheckResponseWriter.WriteResponse
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready"),
    ResponseWriter = VGS.RetailOS.Shared.Observability.HealthChecks.HealthCheckResponseWriter.WriteResponse
});

app.UseHttpsRedirection();

app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    service = "VGS Retail OS API Host",
    status = "running"
}));

// Seed Database on startup if empty
using (var scope = app.Services.CreateScope())
{
    await VGS.RetailOS.Infrastructure.Data.Seeding.DbSeeder.SeedDatabaseAsync(scope.ServiceProvider);
}

app.Run();
