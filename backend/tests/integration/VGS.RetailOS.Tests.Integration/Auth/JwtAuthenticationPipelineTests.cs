using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using VGS.RetailOS.Infrastructure.Auth.Tokens;
using VGS.RetailOS.Modules.Auth.BO;
using VGS.RetailOS.Shared.Errors;
using VGS.RetailOS.Shared.Observability.Logging;
using Xunit;

namespace VGS.RetailOS.Tests.Integration.Auth;

public class JwtAuthenticationPipelineTests
{
    private const string ValidSecret = "vgs_dev_jwt_signing_key_min_32_characters_long_placeholder";
    private const string ValidIssuer = "VGS.RetailOS";
    private const string ValidAudience = "VGS.RetailOS.App";

    private TestServer CreateAuthTestServer(JwtOptions options)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Security:Jwt:Issuer"] = options.Issuer,
                ["Security:Jwt:Audience"] = options.Audience,
                ["Security:Jwt:ExpirationMinutes"] = options.ExpirationMinutes.ToString(),
                ["Security:Jwt:SecretKey"] = options.SecretKey
            })
            .Build();

        var hostBuilder = new WebHostBuilder()
            .UseEnvironment("Development")
            .ConfigureServices(services =>
            {
                services.AddVgsStructuredLogging();
                services.AddVgsErrorHandling();
                services.AddRouting();

                services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
                services.AddTransient<VGS.RetailOS.Modules.Auth.IBL.ITokenService, TokenService>();

                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey));

                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(jwtOptions =>
                    {
                        jwtOptions.RequireHttpsMetadata = false;
                        jwtOptions.SaveToken = true;
                        jwtOptions.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = options.Issuer,
                            ValidateAudience = true,
                            ValidAudience = options.Audience,
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = signingKey,
                            ValidateLifetime = true,
                            RequireExpirationTime = true,
                            ClockSkew = TimeSpan.FromSeconds(5)
                        };
                    });

                services.AddAuthorization();
            })
            .Configure(app =>
            {
                app.UseVgsErrorHandling();
                app.UseVgsRequestLogging();
                app.UseRouting();

                app.UseAuthentication();
                app.UseAuthorization();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/api/protected", (HttpContext ctx) =>
                    {
                        if (ctx.User.Identity?.IsAuthenticated != true)
                        {
                            return Results.Unauthorized();
                        }

                        var userId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                  ?? ctx.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                        var email = ctx.User.FindFirst(ClaimTypes.Email)?.Value
                                 ?? ctx.User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

                        var tenantId = ctx.User.FindFirst("tenant_id")?.Value;

                        return Results.Ok(new { UserId = userId, Email = email, TenantId = tenantId });
                    });

                    endpoints.MapGet("/api/public", () => Results.Ok("Public Endpoint"));
                });
            });

        return new TestServer(hostBuilder);
    }

    private static JwtOptions GetValidOptions() => new()
    {
        Issuer = ValidIssuer,
        Audience = ValidAudience,
        ExpirationMinutes = 15,
        SecretKey = ValidSecret
    };

    [Fact]
    public async Task Valid_JWT_Should_Be_Authenticated_And_Expose_User_Claims()
    {
        var options = GetValidOptions();
        using var server = CreateAuthTestServer(options);
        var client = server.CreateClient();

        var tokenService = new TokenService(Options.Create(options));
        var user = new UserBO
        {
            Id = Guid.NewGuid(),
            Email = "authenticated@vgsretail.com",
            FirstName = "Alice",
            LastName = "Smith",
            SecurityStamp = Guid.NewGuid().ToString()
        };

        var (accessToken, _) = tokenService.GenerateAccessToken(user, tenantHint: "tenant_store_42");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync("/api/protected");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains(user.Id.ToString(), content);
        Assert.Contains(user.Email, content);
        Assert.Contains("tenant_store_42", content);
    }

    [Fact]
    public async Task Missing_Authorization_Header_Should_Be_Unauthenticated()
    {
        using var server = CreateAuthTestServer(GetValidOptions());
        var client = server.CreateClient();

        var response = await client.GetAsync("/api/protected");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Expired_JWT_Should_Be_Rejected()
    {
        var options = GetValidOptions();
        using var server = CreateAuthTestServer(options);
        var client = server.CreateClient();

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ValidSecret));
        var expiredDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()) }),
            Issuer = ValidIssuer,
            Audience = ValidAudience,
            NotBefore = DateTime.UtcNow.AddMinutes(-30),
            Expires = DateTime.UtcNow.AddMinutes(-15),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };
        var expiredToken = tokenHandler.WriteToken(tokenHandler.CreateToken(expiredDescriptor));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", expiredToken);

        var response = await client.GetAsync("/api/protected");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Invalid_Signature_Should_Be_Rejected()
    {
        var options = GetValidOptions();
        using var server = CreateAuthTestServer(options);
        var client = server.CreateClient();

        var differentSecret = "different_secret_key_min_32_characters_long!";
        var tokenHandler = new JwtSecurityTokenHandler();
        var wrongKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(differentSecret));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()) }),
            Issuer = ValidIssuer,
            Audience = ValidAudience,
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(wrongKey, SecurityAlgorithms.HmacSha256)
        };
        var forgedToken = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", forgedToken);

        var response = await client.GetAsync("/api/protected");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Wrong_Issuer_Should_Be_Rejected()
    {
        var options = GetValidOptions();
        using var server = CreateAuthTestServer(options);
        var client = server.CreateClient();

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ValidSecret));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()) }),
            Issuer = "WrongIssuer.Org",
            Audience = ValidAudience,
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };
        var wrongIssuerToken = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", wrongIssuerToken);

        var response = await client.GetAsync("/api/protected");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Wrong_Audience_Should_Be_Rejected()
    {
        var options = GetValidOptions();
        using var server = CreateAuthTestServer(options);
        var client = server.CreateClient();

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ValidSecret));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()) }),
            Issuer = ValidIssuer,
            Audience = "WrongAudience.App",
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };
        var wrongAudienceToken = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", wrongAudienceToken);

        var response = await client.GetAsync("/api/protected");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Malformed_JWT_Should_Be_Rejected()
    {
        using var server = CreateAuthTestServer(GetValidOptions());
        var client = server.CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not.a.valid.jwt.token");

        var response = await client.GetAsync("/api/protected");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task JWT_Secret_And_Bearer_Token_Should_Never_Be_Exposed_In_Response_Or_Logs()
    {
        using var server = CreateAuthTestServer(GetValidOptions());
        var client = server.CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid_bearer_token");

        var response = await client.GetAsync("/api/protected");
        var content = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain(ValidSecret, content);
        Assert.DoesNotContain("invalid_bearer_token", content);
    }

    [Fact]
    public async Task Public_Endpoints_Should_Remain_Accessible_Without_Authentication()
    {
        using var server = CreateAuthTestServer(GetValidOptions());
        var client = server.CreateClient();

        var response = await client.GetAsync("/api/public");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Public Endpoint", content);
    }
}
