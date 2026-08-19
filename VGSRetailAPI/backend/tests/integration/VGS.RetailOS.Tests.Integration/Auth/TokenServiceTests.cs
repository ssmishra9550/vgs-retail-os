using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using VGS.RetailOS.Infrastructure.Auth.Tokens;
using VGS.RetailOS.Modules.Auth.BO;
using Xunit;

namespace VGS.RetailOS.Tests.Integration.Auth;

public class TokenServiceTests
{
    private const string ValidSecretKey = "vgs_dev_jwt_signing_key_min_32_characters_long_placeholder";
    private const string ValidIssuer = "VGS.RetailOS";
    private const string ValidAudience = "VGS.RetailOS.App";

    private readonly JwtOptions _validOptions = new()
    {
        Issuer = ValidIssuer,
        Audience = ValidAudience,
        ExpirationMinutes = 15,
        SecretKey = ValidSecretKey
    };

    [Fact]
    public void GenerateRefreshToken_Should_Return_256Bit_Entropy_RawToken_And_Valid_SHA256_Hash()
    {
        var sut = CreateTokenService(_validOptions);

        var (rawToken, tokenHash) = sut.GenerateRefreshToken();

        Assert.False(string.IsNullOrWhiteSpace(rawToken));
        Assert.False(string.IsNullOrWhiteSpace(tokenHash));
        Assert.Equal(64, tokenHash.Length); // 32 bytes SHA-256 = 64 hex characters

        // Decode raw token Base64Url bytes and verify exact 32 random bytes (256-bit entropy)
        var tokenBytes = DecodeBase64Url(rawToken);
        Assert.Equal(32, tokenBytes.Length);

        // Verify SHA-256 hash matches deterministic HashToken output
        var expectedHash = sut.HashToken(rawToken);
        Assert.Equal(expectedHash, tokenHash);
    }

    [Fact]
    public void GenerateRefreshToken_Should_Produce_Unique_Tokens()
    {
        var sut = CreateTokenService(_validOptions);

        var token1 = sut.GenerateRefreshToken();
        var token2 = sut.GenerateRefreshToken();

        Assert.NotEqual(token1.RawToken, token2.RawToken);
        Assert.NotEqual(token1.TokenHash, token2.TokenHash);
    }

    [Fact]
    public void HashToken_Should_Be_Deterministic_SHA256_Hex_String()
    {
        var sut = CreateTokenService(_validOptions);
        var input = "sample_raw_refresh_token_string";

        var hash1 = sut.HashToken(input);
        var hash2 = sut.HashToken(input);

        Assert.Equal(hash1, hash2);
        Assert.Equal(64, hash1.Length);

        // Compute manually with SHA256 to verify exact correctness
        var expectedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var expectedHex = Convert.ToHexStringLower(expectedBytes);
        Assert.Equal(expectedHex, hash1);
    }

    [Fact]
    public void GenerateAccessToken_Should_Produce_Valid_JWT_With_15_Minute_Lifetime_And_Expected_Claims()
    {
        var sut = CreateTokenService(_validOptions);
        var user = CreateTestUser();

        var (accessToken, expiresAt) = sut.GenerateAccessToken(user, tenantHint: "tenant_store_01");

        Assert.False(string.IsNullOrWhiteSpace(accessToken));
        Assert.True(expiresAt > DateTimeOffset.UtcNow);
        Assert.True(expiresAt <= DateTimeOffset.UtcNow.AddMinutes(15).AddSeconds(5));

        // Validate JWT signature and claims using JwtSecurityTokenHandler
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ValidSecretKey)),
            ValidateIssuer = true,
            ValidIssuer = ValidIssuer,
            ValidateAudience = true,
            ValidAudience = ValidAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(5)
        };

        var principal = tokenHandler.ValidateToken(accessToken, validationParameters, out var validatedToken);

        Assert.NotNull(validatedToken);
        Assert.IsType<JwtSecurityToken>(validatedToken);

        // Assert claims
        Assert.Equal(user.Id.ToString(), principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value);
        Assert.Equal(user.Email, principal.FindFirst(ClaimTypes.Email)?.Value ?? principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value);
        Assert.Equal(user.FirstName, principal.FindFirst(ClaimTypes.GivenName)?.Value ?? principal.FindFirst(JwtRegisteredClaimNames.GivenName)?.Value);
        Assert.Equal(user.LastName, principal.FindFirst(ClaimTypes.Surname)?.Value ?? principal.FindFirst(JwtRegisteredClaimNames.FamilyName)?.Value);
        Assert.Equal(user.SecurityStamp, principal.FindFirst("sec_stamp")?.Value);
        Assert.Equal("tenant_store_01", principal.FindFirst("tenant_id")?.Value);
    }

    [Theory]
    [InlineData("", "Missing SecretKey")]
    [InlineData("short_key_under_32_chars!", "Short SecretKey")]
    public void Startup_Validation_Should_Throw_InvalidOperationException_When_SecretKey_Invalid(string invalidKey, string reason)
    {
        Assert.NotNull(reason);
        var options = new JwtOptions
        {
            Issuer = ValidIssuer,
            Audience = ValidAudience,
            ExpirationMinutes = 15,
            SecretKey = invalidKey
        };

        var ex = Assert.Throws<InvalidOperationException>(() => CreateTokenService(options));
        Assert.Contains("JWT SecretKey", ex.Message);
    }

    [Fact]
    public void Startup_Validation_Should_Throw_InvalidOperationException_When_ExpirationMinutes_Not_15()
    {
        var options = new JwtOptions
        {
            Issuer = ValidIssuer,
            Audience = ValidAudience,
            ExpirationMinutes = 60, // Invalid: must be 15
            SecretKey = ValidSecretKey
        };

        var ex = Assert.Throws<InvalidOperationException>(() => CreateTokenService(options));
        Assert.Contains("ExpirationMinutes must be set to exactly 15 minutes", ex.Message);
    }

    [Fact]
    public void Startup_Validation_Should_Throw_InvalidOperationException_When_Issuer_Or_Audience_Missing()
    {
        var missingIssuer = new JwtOptions { Issuer = "", Audience = ValidAudience, ExpirationMinutes = 15, SecretKey = ValidSecretKey };
        var missingAudience = new JwtOptions { Issuer = ValidIssuer, Audience = "", ExpirationMinutes = 15, SecretKey = ValidSecretKey };

        Assert.Throws<InvalidOperationException>(() => CreateTokenService(missingIssuer));
        Assert.Throws<InvalidOperationException>(() => CreateTokenService(missingAudience));
    }

    private static TokenService CreateTokenService(JwtOptions options)
    {
        return new TokenService(Options.Create(options));
    }

    private static UserBO CreateTestUser() => new()
    {
        Id = Guid.NewGuid(),
        Email = "testuser@vgsretail.com",
        FirstName = "Jane",
        LastName = "Doe",
        IsActive = true,
        SecurityStamp = Guid.NewGuid().ToString(),
        CreatedAt = DateTimeOffset.UtcNow
    };

    private static byte[] DecodeBase64Url(string base64Url)
    {
        var padded = base64Url.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }
        return Convert.FromBase64String(padded);
    }
}
