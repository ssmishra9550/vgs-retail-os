using Microsoft.AspNetCore.Identity;
using VGS.RetailOS.Infrastructure.Auth.DAC.Entities;
using VGS.RetailOS.Modules.Auth.IBL;
using DomainResult = VGS.RetailOS.Modules.Auth.IBL.PasswordVerificationResult;
using IdentityResult = Microsoft.AspNetCore.Identity.PasswordVerificationResult;

namespace VGS.RetailOS.Infrastructure.Auth.DAC;

/// <summary>
/// Infrastructure implementation of IPasswordVerifier using ASP.NET Core Identity's IPasswordHasher.
/// Decouples business logic (AuthBL) from framework-specific Identity types.
/// </summary>
public class IdentityPasswordVerifier : IPasswordVerifier
{
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
    private static readonly ApplicationUser DummyUser = new();

    public IdentityPasswordVerifier(IPasswordHasher<ApplicationUser> passwordHasher)
    {
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    public DomainResult VerifyPassword(string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword) || string.IsNullOrEmpty(providedPassword))
        {
            return DomainResult.Failed;
        }

        var result = _passwordHasher.VerifyHashedPassword(DummyUser, hashedPassword, providedPassword);

        return result switch
        {
            IdentityResult.Success => DomainResult.Success,
            IdentityResult.SuccessRehashNeeded => DomainResult.SuccessRehashNeeded,
            _ => DomainResult.Failed
        };
    }
}
