using VGS.RetailOS.Modules.Auth.BO;
using Xunit;

namespace VGS.RetailOS.Tests.Unit.Auth;

public class AuthBOTests
{
    [Fact]
    public void UserBO_FullName_Should_Combine_FirstName_And_LastName()
    {
        var user = new UserBO
        {
            Id = Guid.NewGuid(),
            Email = "john.doe@vgsretail.com",
            FirstName = "John",
            LastName = "Doe",
            SecurityStamp = Guid.NewGuid().ToString()
        };

        Assert.Equal("John Doe", user.FullName);
    }

    [Fact]
    public void RefreshTokenBO_IsActive_Should_Return_True_For_Valid_Non_Revoked_Token()
    {
        var now = DateTimeOffset.UtcNow;
        var token = new RefreshTokenBO
        {
            Id = Guid.NewGuid(),
            TokenHash = "hash123",
            UserId = Guid.NewGuid(),
            FamilyId = Guid.NewGuid(),
            ExpiresAt = now.AddDays(7),
            IsRevoked = false
        };

        Assert.True(token.IsActive(now));
        Assert.False(token.IsExpired(now));
    }

    [Fact]
    public void RefreshTokenBO_IsActive_Should_Return_False_When_Revoked()
    {
        var now = DateTimeOffset.UtcNow;
        var token = new RefreshTokenBO
        {
            Id = Guid.NewGuid(),
            TokenHash = "hash123",
            UserId = Guid.NewGuid(),
            FamilyId = Guid.NewGuid(),
            ExpiresAt = now.AddDays(7),
            IsRevoked = true,
            RevocationReason = "User Logout"
        };

        Assert.False(token.IsActive(now));
    }

    [Fact]
    public void RefreshTokenBO_IsActive_Should_Return_False_When_Expired()
    {
        var now = DateTimeOffset.UtcNow;
        var token = new RefreshTokenBO
        {
            Id = Guid.NewGuid(),
            TokenHash = "hash123",
            UserId = Guid.NewGuid(),
            FamilyId = Guid.NewGuid(),
            ExpiresAt = now.AddMinutes(-5),
            IsRevoked = false
        };

        Assert.False(token.IsActive(now));
        Assert.True(token.IsExpired(now));
    }
}
