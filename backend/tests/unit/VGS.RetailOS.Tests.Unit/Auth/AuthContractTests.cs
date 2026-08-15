using VGS.RetailOS.Modules.Auth.BO;
using VGS.RetailOS.Modules.Auth.IBL;
using VGS.RetailOS.Modules.Auth.IDAC;
using Xunit;

namespace VGS.RetailOS.Tests.Unit.Auth;

public class AuthContractTests
{
    [Fact]
    public void IAuthBL_Should_Define_Required_UseCases()
    {
        var methods = typeof(IAuthBL).GetMethods();

        Assert.Contains(methods, m => m.Name == nameof(IAuthBL.LoginAsync));
        Assert.Contains(methods, m => m.Name == nameof(IAuthBL.RefreshTokenAsync));
        Assert.Contains(methods, m => m.Name == nameof(IAuthBL.LogoutAsync));
        Assert.Contains(methods, m => m.Name == nameof(IAuthBL.GetCurrentUserAsync));
    }

    [Fact]
    public void IPasswordVerifier_Should_Define_VerifyPassword()
    {
        var method = typeof(IPasswordVerifier).GetMethod(nameof(IPasswordVerifier.VerifyPassword));

        Assert.NotNull(method);
        Assert.Equal(typeof(PasswordVerificationResult), method.ReturnType);
        Assert.Equal(2, method.GetParameters().Length);
    }

    [Fact]
    public void IAuthDAC_Should_Define_Persistence_Operations()
    {
        var methods = typeof(IAuthDAC).GetMethods();

        Assert.Contains(methods, m => m.Name == nameof(IAuthDAC.FindUserSecurityInfoByEmailAsync));
        Assert.Contains(methods, m => m.Name == nameof(IAuthDAC.GetUserByIdAsync));
        Assert.Contains(methods, m => m.Name == nameof(IAuthDAC.VerifyTenantMembershipAsync));
        Assert.Contains(methods, m => m.Name == nameof(IAuthDAC.RecordLoginSuccessAsync));
        Assert.Contains(methods, m => m.Name == nameof(IAuthDAC.RecordLoginFailureAsync));
        Assert.Contains(methods, m => m.Name == nameof(IAuthDAC.GetRefreshTokenByHashAsync));
        Assert.Contains(methods, m => m.Name == nameof(IAuthDAC.SaveRefreshTokenAsync));
        Assert.Contains(methods, m => m.Name == nameof(IAuthDAC.RotateRefreshTokenAsync));
        Assert.Contains(methods, m => m.Name == nameof(IAuthDAC.RevokeTokenFamilyAsync));
        Assert.Contains(methods, m => m.Name == nameof(IAuthDAC.RevokeRefreshTokenAsync));
    }

    [Fact]
    public void UserSecurityInfoBO_IsLockedOut_Should_Evaluate_LockoutEnd()
    {
        var now = DateTimeOffset.UtcNow;
        var info = new UserSecurityInfoBO
        {
            User = new UserBO { Id = Guid.NewGuid() },
            PasswordHash = "hashedPassword",
            AccessFailedCount = 5,
            LockoutEnd = now.AddMinutes(15)
        };

        Assert.True(info.IsLockedOut(now));
    }
}
