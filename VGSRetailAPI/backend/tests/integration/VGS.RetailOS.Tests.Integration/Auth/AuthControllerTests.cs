using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VGS.RetailOS.ApiHost.Contracts.V1.Auth;
using VGS.RetailOS.Infrastructure.Auth.DAC.Entities;
using VGS.RetailOS.Infrastructure.Data;
using Xunit;

namespace VGS.RetailOS.Tests.Integration.Auth;

public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private AppDbContext _dbContext = default!;

    public AuthControllerTests(WebApplicationFactory<Program> factory)
    {
        var pgHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "127.0.0.1";
        var pgPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5435";
        var pgDb = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "vgs_retail_os_dev";
        var pgUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "vgs_dev";
        var pgPass = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "vgs_dev_password_placeholder";
        var pgConnectionString = $"Host={pgHost};Port={pgPort};Database={pgDb};Username={pgUser};Password={pgPass};Ssl Mode=Disable;";

        var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "127.0.0.1";
        var redisPort = Environment.GetEnvironmentVariable("REDIS_PORT") ?? "6380";
        var redisConnectionString = $"{redisHost}:{redisPort},abortConnect=false";

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Redis", redisConnectionString);
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(pgConnectionString));
            });
        });

        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        var scope = _factory.Services.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        await _dbContext.Database.MigrateAsync();
        
        // Ensure clean test state for users
        _dbContext.Users.RemoveRange(_dbContext.Users);
        await _dbContext.SaveChangesAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkAndTokens()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var user = new ApplicationUser
        {
            UserName = "test.login@vgs.local",
            Email = "test.login@vgs.local",
            FirstName = "Test",
            LastName = "Login"
        };
        
        await userManager.CreateAsync(user, "Password123!");

        var request = new LoginRequest("test.login@vgs.local", "Password123!", null);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        
        Assert.NotNull(content);
        Assert.NotEmpty(content.AccessToken);
        Assert.NotEmpty(content.RefreshToken);
        Assert.Equal(user.Id, content.User.Id);
        Assert.Equal(user.Email, content.User.Email);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest("wrong@vgs.local", "wrongpassword", null);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
