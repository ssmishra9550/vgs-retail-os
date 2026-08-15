using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using VGS.RetailOS.Modules.Auth.BO;
using VGS.RetailOS.Modules.Auth.IBL;

namespace VGS.RetailOS.Infrastructure.Auth.Tokens;

/// <summary>
/// Concrete infrastructure implementation of ITokenService.
/// Encapsulates all JWT creation, signing-key handling, cryptographically secure random token generation,
/// and SHA-256 token hashing. Modules layer remains 100% decoupled from this infrastructure service.
/// </summary>
public sealed class TokenService : ITokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public TokenService(IOptions<JwtOptions> jwtOptions)
    {
        ArgumentNullException.ThrowIfNull(jwtOptions);
        _jwtOptions = jwtOptions.Value;
        _jwtOptions.Validate();
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    public string HashToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));
        }

        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(tokenBytes);
        return Convert.ToHexStringLower(hashBytes);
    }

    public (string RawToken, string TokenHash) GenerateRefreshToken()
    {
        // ADR-005 Requirement: Exactly 256-bit entropy (32 random bytes) via RandomNumberGenerator
        var randomBytes = new byte[32];
        RandomNumberGenerator.Fill(randomBytes);
        var rawToken = EncodeBase64Url(randomBytes);
        var tokenHash = HashToken(rawToken);

        return (rawToken, tokenHash);
    }

    public (string AccessToken, DateTimeOffset ExpiresAt) GenerateAccessToken(UserBO user, string? tenantHint = null)
    {
        ArgumentNullException.ThrowIfNull(user);

        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(_jwtOptions.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName ?? string.Empty),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName ?? string.Empty),
            new("sec_stamp", user.SecurityStamp ?? string.Empty)
        };

        if (!string.IsNullOrWhiteSpace(tenantHint))
        {
            claims.Add(new Claim("tenant_id", tenantHint));
        }

        var keyBytes = Encoding.UTF8.GetBytes(_jwtOptions.SecretKey);
        var securityKey = new SymmetricSecurityKey(keyBytes);
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = signingCredentials
        };

        var securityToken = _tokenHandler.CreateToken(tokenDescriptor);
        var accessToken = _tokenHandler.WriteToken(securityToken);

        return (accessToken, expiresAt);
    }

    private static string EncodeBase64Url(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
