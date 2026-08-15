using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace VGS.RetailOS.Tests.Integration.Observability;

public class HealthCheckTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthCheckTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Liveness_Should_Return_Healthy()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        
        using var doc = JsonDocument.Parse(content);
        var status = doc.RootElement.GetProperty("status").GetString();
        
        Assert.Equal("Healthy", status);
    }

    [Fact]
    public async Task Readiness_Should_Return_Healthy_When_Dependencies_Available()
    {
        var pgHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "127.0.0.1";
        var pgPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5435";
        var pgDb = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "vgs_retail_os_dev";
        var pgUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "vgs_dev";
        var pgPass = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "vgs_dev_password_placeholder";

        var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "127.0.0.1";
        var redisPort = Environment.GetEnvironmentVariable("REDIS_PORT") ?? "6379";

        var pgConnectionString = $"Host={pgHost};Port={pgPort};Database={pgDb};Username={pgUser};Password={pgPass};Ssl Mode=Disable;";
        var redisConnectionString = $"{redisHost}:{redisPort},abortConnect=false";

        var envFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(Microsoft.EntityFrameworkCore.DbContextOptions<VGS.RetailOS.Infrastructure.Data.AppDbContext>));
                if (dbContextDescriptor != null) services.Remove(dbContextDescriptor);

                services.AddDbContext<VGS.RetailOS.Infrastructure.Data.AppDbContext>(options =>
                    options.UseNpgsql(pgConnectionString));

                var redisDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(StackExchange.Redis.IConnectionMultiplexer));
                if (redisDescriptor != null) services.Remove(redisDescriptor);

                services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp => StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnectionString));
            });
        });
        
        var client = envFactory.CreateClient();

        var response = await client.GetAsync("/health/ready");
        var content = await response.Content.ReadAsStringAsync();
        
        using var doc = JsonDocument.Parse(content);
        var status = doc.RootElement.GetProperty("status").GetString();
        var entries = doc.RootElement.GetProperty("entries");
        
        Assert.True(entries.TryGetProperty("postgresql", out var pgEntry), "PostgreSQL entry missing");
        Assert.True(pgEntry.GetProperty("status").GetString() == "Healthy", $"PostgreSQL not Healthy: {content}");
        
        Assert.True(entries.TryGetProperty("redis", out var redisEntry), "Redis entry missing");
        Assert.True(redisEntry.GetProperty("status").GetString() == "Healthy", $"Redis not Healthy: {content}");
        
        Assert.Equal("Healthy", status);
    }

    [Fact]
    public async Task Readiness_Should_Fail_When_Database_Unavailable_But_Liveness_Succeeds()
    {
        var badFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(Microsoft.EntityFrameworkCore.DbContextOptions<VGS.RetailOS.Infrastructure.Data.AppDbContext>));
                if (dbContextDescriptor != null) services.Remove(dbContextDescriptor);

                services.AddDbContext<VGS.RetailOS.Infrastructure.Data.AppDbContext>(options =>
                    options.UseNpgsql("Host=invalid-host;Port=5432;Database=vgs_retail_os;Username=postgres;Password=postgres;Timeout=2"));

                var redisDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(StackExchange.Redis.IConnectionMultiplexer));
                if (redisDescriptor != null) services.Remove(redisDescriptor);

                // Redis connection string configured to timeout quickly
                services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp => StackExchange.Redis.ConnectionMultiplexer.Connect("invalid-host:6379,abortConnect=false,connectTimeout=2000"));
            });
        });

        var client = badFactory.CreateClient();

        // Liveness should still succeed
        var liveResponse = await client.GetAsync("/health/live");
        liveResponse.EnsureSuccessStatusCode();
        var liveContent = await liveResponse.Content.ReadAsStringAsync();
        using var liveDoc = JsonDocument.Parse(liveContent);
        Assert.Equal("Healthy", liveDoc.RootElement.GetProperty("status").GetString());

        // Readiness should fail
        var readyResponse = await client.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, readyResponse.StatusCode);
        
        var readyContent = await readyResponse.Content.ReadAsStringAsync();
        using var readyDoc = JsonDocument.Parse(readyContent);
        
        var status = readyDoc.RootElement.GetProperty("status").GetString();
        Assert.Equal("Unhealthy", status);

        // Verify secrets are not exposed in failure messages
        Assert.DoesNotContain("invalid-host", readyContent);
        Assert.DoesNotContain("Password", readyContent);
    }
}
