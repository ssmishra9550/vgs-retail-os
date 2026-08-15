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

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

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

        services.Configure<VGS.RetailOS.Infrastructure.Auth.Tokens.JwtOptions>(configuration.GetSection(VGS.RetailOS.Infrastructure.Auth.Tokens.JwtOptions.SectionName));
        services.AddTransient<VGS.RetailOS.Modules.Auth.IBL.ITokenService, VGS.RetailOS.Infrastructure.Auth.Tokens.TokenService>();

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
