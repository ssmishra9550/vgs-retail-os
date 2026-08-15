using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VGS.RetailOS.Infrastructure.Auth.DAC.Entities;
using VGS.RetailOS.Infrastructure.Data;
using VGS.RetailOS.Modules.Auth.BO;
using VGS.RetailOS.Modules.Auth.IDAC;
using Xunit;

namespace VGS.RetailOS.Tests.Integration.Auth;

public class AuthDACTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthDACTests(WebApplicationFactory<Program> factory)
    {
        var pgHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "127.0.0.1";
        var pgPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5435";
        var pgDb = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "vgs_retail_os_dev";
        var pgUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "vgs_dev";
        var pgPass = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "vgs_dev_password_placeholder";
        var pgConnectionString = $"Host={pgHost};Port={pgPort};Database={pgDb};Username={pgUser};Password={pgPass};Ssl Mode=Disable;";

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(pgConnectionString));
            });
        });
    }

    [Fact]
    public async Task FindUserSecurityInfoByEmailAsync_Should_Return_UserSecurityInfoBO()
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var authDac = scope.ServiceProvider.GetRequiredService<IAuthDAC>();

        var testEmail = $"dac_test_{Guid.NewGuid():N}@vgsretail.com";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = testEmail,
            UserName = testEmail,
            NormalizedEmail = testEmail.ToUpperInvariant(),
            NormalizedUserName = testEmail.ToUpperInvariant(),
            FirstName = "DAC",
            LastName = "User",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var createResult = await userManager.CreateAsync(user, "SecureP@ssword123!");
        Assert.True(createResult.Succeeded);

        var securityInfo = await authDac.FindUserSecurityInfoByEmailAsync(testEmail);

        Assert.NotNull(securityInfo);
        Assert.Equal(testEmail, securityInfo.User.Email);
        Assert.False(string.IsNullOrWhiteSpace(securityInfo.PasswordHash));
        Assert.IsType<UserSecurityInfoBO>(securityInfo);
        Assert.IsType<UserBO>(securityInfo.User);
    }

    [Fact]
    public async Task GetUserByIdAsync_Should_Return_UserBO()
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var authDac = scope.ServiceProvider.GetRequiredService<IAuthDAC>();

        var testEmail = $"lookup_{Guid.NewGuid():N}@vgsretail.com";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = testEmail,
            UserName = testEmail,
            NormalizedEmail = testEmail.ToUpperInvariant(),
            NormalizedUserName = testEmail.ToUpperInvariant(),
            FirstName = "Lookup",
            LastName = "User",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await userManager.CreateAsync(user, "SecureP@ssword123!");

        var userBO = await authDac.GetUserByIdAsync(user.Id);

        Assert.NotNull(userBO);
        Assert.Equal(user.Id, userBO.Id);
        Assert.Equal(testEmail, userBO.Email);
        Assert.IsType<UserBO>(userBO);
    }

    [Fact]
    public async Task RecordLoginSuccessAndFailure_Should_Update_Persistence_State()
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var authDac = scope.ServiceProvider.GetRequiredService<IAuthDAC>();

        var testEmail = $"login_state_{Guid.NewGuid():N}@vgsretail.com";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = testEmail,
            UserName = testEmail,
            NormalizedEmail = testEmail.ToUpperInvariant(),
            NormalizedUserName = testEmail.ToUpperInvariant(),
            FirstName = "State",
            LastName = "User",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await userManager.CreateAsync(user, "SecureP@ssword123!");

        // Record failure
        var lockoutEnd = DateTimeOffset.UtcNow.AddMinutes(15);
        await authDac.RecordLoginFailureAsync(user.Id, 3, lockoutEnd);

        var failedSecurityInfo = await authDac.FindUserSecurityInfoByEmailAsync(testEmail);
        Assert.NotNull(failedSecurityInfo);
        Assert.Equal(3, failedSecurityInfo.AccessFailedCount);
        Assert.NotNull(failedSecurityInfo.LockoutEnd);

        // Record success
        var loginTime = DateTimeOffset.UtcNow;
        await authDac.RecordLoginSuccessAsync(user.Id, loginTime);

        var successSecurityInfo = await authDac.FindUserSecurityInfoByEmailAsync(testEmail);
        Assert.NotNull(successSecurityInfo);
        Assert.Equal(0, successSecurityInfo.AccessFailedCount);
        Assert.Null(successSecurityInfo.LockoutEnd);
        Assert.NotNull(successSecurityInfo.User.LastLoginAt);
    }

    [Fact]
    public async Task RefreshToken_Lifecycle_Operations_Should_Persist_And_Rotate()
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var authDac = scope.ServiceProvider.GetRequiredService<IAuthDAC>();

        var testEmail = $"token_user_{Guid.NewGuid():N}@vgsretail.com";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = testEmail,
            UserName = testEmail,
            NormalizedEmail = testEmail.ToUpperInvariant(),
            NormalizedUserName = testEmail.ToUpperInvariant(),
            FirstName = "Token",
            LastName = "User",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await userManager.CreateAsync(user, "SecureP@ssword123!");

        var familyId = Guid.NewGuid();
        var initialHash = $"hash_1_{Guid.NewGuid():N}";
        var initialToken = new RefreshTokenBO
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = initialHash,
            FamilyId = familyId,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            IsRevoked = false,
            ReplacedByTokenHash = null,
            CreatedFromIp = "127.0.0.1",
            UserAgent = "TestAgent",
            CreatedAt = DateTimeOffset.UtcNow,
            RevokedAt = null,
            RevocationReason = null
        };

        // Save
        await authDac.SaveRefreshTokenAsync(initialToken);

        var fetchedToken = await authDac.GetRefreshTokenByHashAsync(initialHash);
        Assert.NotNull(fetchedToken);
        Assert.Equal(initialHash, fetchedToken.TokenHash);
        Assert.False(fetchedToken.IsRevoked);

        // Rotate
        var newHash = $"hash_2_{Guid.NewGuid():N}";
        var rotatedToken = new RefreshTokenBO
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = newHash,
            FamilyId = familyId,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            IsRevoked = false,
            ReplacedByTokenHash = null,
            CreatedFromIp = "127.0.0.1",
            UserAgent = "TestAgent",
            CreatedAt = DateTimeOffset.UtcNow,
            RevokedAt = null,
            RevocationReason = null
        };

        await authDac.RotateRefreshTokenAsync(initialHash, rotatedToken);

        var oldTokenState = await authDac.GetRefreshTokenByHashAsync(initialHash);
        Assert.NotNull(oldTokenState);
        Assert.True(oldTokenState.IsRevoked);
        Assert.Equal(newHash, oldTokenState.ReplacedByTokenHash);

        var newTokenState = await authDac.GetRefreshTokenByHashAsync(newHash);
        Assert.NotNull(newTokenState);
        Assert.False(newTokenState.IsRevoked);

        // Double rotation attempt should fail safely
        var duplicateRotatedToken = new RefreshTokenBO
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = $"hash_3_{Guid.NewGuid():N}",
            FamilyId = familyId,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            IsRevoked = false
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            authDac.RotateRefreshTokenAsync(initialHash, duplicateRotatedToken));

        // Revoke family
        await authDac.RevokeTokenFamilyAsync(familyId, "FamilyCompromised");

        var familyTokenState = await authDac.GetRefreshTokenByHashAsync(newHash);
        Assert.NotNull(familyTokenState);
        Assert.True(familyTokenState.IsRevoked);
        Assert.Equal("FamilyCompromised", familyTokenState.RevocationReason);
    }

    [Fact]
    public async Task RotateRefreshTokenAsync_Concurrent_Rotation_Should_Allow_Only_One_Success()
    {
        using var scopeSetup = _factory.Services.CreateScope();
        var userManager = scopeSetup.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var authDacSetup = scopeSetup.ServiceProvider.GetRequiredService<IAuthDAC>();

        var testEmail = $"concurrent_user_{Guid.NewGuid():N}@vgsretail.com";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = testEmail,
            UserName = testEmail,
            NormalizedEmail = testEmail.ToUpperInvariant(),
            NormalizedUserName = testEmail.ToUpperInvariant(),
            FirstName = "Concurrent",
            LastName = "User",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await userManager.CreateAsync(user, "SecureP@ssword123!");

        var familyId = Guid.NewGuid();
        var initialHash = $"conc_hash_{Guid.NewGuid():N}";
        var initialToken = new RefreshTokenBO
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = initialHash,
            FamilyId = familyId,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await authDacSetup.SaveRefreshTokenAsync(initialToken);

        // Prepare two concurrent rotation attempts with different target replacement tokens
        var tokenA = new RefreshTokenBO
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = $"replacement_A_{Guid.NewGuid():N}",
            FamilyId = familyId,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            IsRevoked = false
        };

        var tokenB = new RefreshTokenBO
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = $"replacement_B_{Guid.NewGuid():N}",
            FamilyId = familyId,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            IsRevoked = false
        };

        Task<bool> RotateTask(RefreshTokenBO replacementToken) => Task.Run(async () =>
        {
            using var scope = _factory.Services.CreateScope();
            var dac = scope.ServiceProvider.GetRequiredService<IAuthDAC>();
            try
            {
                await dac.RotateRefreshTokenAsync(initialHash, replacementToken);
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        });

        var taskA = RotateTask(tokenA);
        var taskB = RotateTask(tokenB);

        var results = await Task.WhenAll(taskA, taskB);

        // Verify exactly one succeeded and one failed
        Assert.Single(results, res => res == true);
        Assert.Single(results, res => res == false);
    }

    [Fact]
    public async Task VerifyTenantMembershipAsync_Should_Throw_NotSupportedException_Until_Tenant_Module_Exists()
    {
        using var scope = _factory.Services.CreateScope();
        var authDac = scope.ServiceProvider.GetRequiredService<IAuthDAC>();

        await Assert.ThrowsAsync<NotSupportedException>(() =>
            authDac.VerifyTenantMembershipAsync(Guid.NewGuid(), "vgs_store_1"));
    }
}
