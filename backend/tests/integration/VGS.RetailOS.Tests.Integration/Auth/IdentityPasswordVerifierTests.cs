using Microsoft.AspNetCore.Identity;
using VGS.RetailOS.Infrastructure.Auth.DAC;
using VGS.RetailOS.Infrastructure.Auth.DAC.Entities;
using DomainResult = VGS.RetailOS.Modules.Auth.IBL.PasswordVerificationResult;
using Xunit;

namespace VGS.RetailOS.Tests.Integration.Auth;

public class IdentityPasswordVerifierTests
{
    private readonly IPasswordHasher<ApplicationUser> _hasher = new PasswordHasher<ApplicationUser>();
    private readonly IdentityPasswordVerifier _verifier;

    public IdentityPasswordVerifierTests()
    {
        _verifier = new IdentityPasswordVerifier(_hasher);
    }

    [Fact]
    public void VerifyPassword_Should_Return_Success_For_Valid_Password()
    {
        var dummyUser = new ApplicationUser();
        var hashedPassword = _hasher.HashPassword(dummyUser, "ValidP@ssword123");

        var result = _verifier.VerifyPassword(hashedPassword, "ValidP@ssword123");

        Assert.Equal(DomainResult.Success, result);
    }

    [Fact]
    public void VerifyPassword_Should_Return_Failed_For_Invalid_Password()
    {
        var dummyUser = new ApplicationUser();
        var hashedPassword = _hasher.HashPassword(dummyUser, "ValidP@ssword123");

        var result = _verifier.VerifyPassword(hashedPassword, "WrongPassword123");

        Assert.Equal(DomainResult.Failed, result);
    }

    [Fact]
    public void VerifyPassword_Should_Return_Failed_When_Inputs_Null_Or_Empty()
    {
        Assert.Equal(DomainResult.Failed, _verifier.VerifyPassword("", "Provided"));
        Assert.Equal(DomainResult.Failed, _verifier.VerifyPassword("Hashed", ""));
    }
}
