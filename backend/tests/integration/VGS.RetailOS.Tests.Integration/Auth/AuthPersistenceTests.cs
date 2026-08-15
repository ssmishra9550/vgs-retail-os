using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VGS.RetailOS.Infrastructure.Auth.DAC.Entities;
using VGS.RetailOS.Infrastructure.Data;
using Xunit;

namespace VGS.RetailOS.Tests.Integration.Auth;

public class AuthPersistenceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthPersistenceTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void IdentityServices_Should_Resolve_From_DI()
    {
        using var scope = _factory.Services.CreateScope();
        
        var userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetService<RoleManager<ApplicationRole>>();
        var dbContext = scope.ServiceProvider.GetService<AppDbContext>();

        Assert.NotNull(userManager);
        Assert.NotNull(roleManager);
        Assert.NotNull(dbContext);
    }

    [Fact]
    public void AppDbContext_Model_Should_Configure_Identity_And_RefreshToken_Entities()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var model = dbContext.Model;

        var userEntity = model.FindEntityType(typeof(ApplicationUser));
        var roleEntity = model.FindEntityType(typeof(ApplicationRole));
        var tokenEntity = model.FindEntityType(typeof(RefreshTokenEntity));

        Assert.NotNull(userEntity);
        Assert.NotNull(roleEntity);
        Assert.NotNull(tokenEntity);

        Assert.Equal("users", userEntity.GetTableName());
        Assert.Equal("roles", roleEntity.GetTableName());
        Assert.Equal("refresh_tokens", tokenEntity.GetTableName());

        // Verify Guid Key Types
        var userPk = userEntity.FindPrimaryKey();
        Assert.NotNull(userPk);
        Assert.Equal(typeof(Guid), userPk.Properties[0].ClrType);

        var rolePk = roleEntity.FindPrimaryKey();
        Assert.NotNull(rolePk);
        Assert.Equal(typeof(Guid), rolePk.Properties[0].ClrType);
    }

    [Fact]
    public void RefreshTokenEntity_Should_Store_TokenHash_And_Not_PlaintextToken()
    {
        var properties = typeof(RefreshTokenEntity).GetProperties();

        Assert.Contains(properties, p => p.Name == nameof(RefreshTokenEntity.TokenHash));
        Assert.Contains(properties, p => p.Name == nameof(RefreshTokenEntity.FamilyId));
        Assert.Contains(properties, p => p.Name == nameof(RefreshTokenEntity.ExpiresAt));
        Assert.Contains(properties, p => p.Name == nameof(RefreshTokenEntity.IsRevoked));
        Assert.DoesNotContain(properties, p => p.Name.Equals("PlaintextToken", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(properties, p => p.Name.Equals("RawToken", StringComparison.OrdinalIgnoreCase));
    }
}
