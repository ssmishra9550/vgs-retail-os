namespace VGS.RetailOS.Modules.Auth.IBL;

/// <summary>
/// Framework-independent password verification abstraction.
/// Decouples business logic (AuthBL) from infrastructure identity providers and ASP.NET Core Identity types.
/// </summary>
public interface IPasswordVerifier
{
    PasswordVerificationResult VerifyPassword(string hashedPassword, string providedPassword);
}

public enum PasswordVerificationResult
{
    Failed = 0,
    Success = 1,
    SuccessRehashNeeded = 2
}
