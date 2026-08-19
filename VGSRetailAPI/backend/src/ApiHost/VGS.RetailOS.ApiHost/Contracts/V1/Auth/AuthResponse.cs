namespace VGS.RetailOS.ApiHost.Contracts.V1.Auth;

public sealed record AuthResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    UserDto User);

public sealed record UserDto(Guid Id, string Email, string FirstName, string LastName);
