using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using VGS.RetailOS.Infrastructure;
using VGS.RetailOS.Shared.BuildingBlocks.Caching;

namespace VGS.RetailOS.Tests.Integration.Caching;

public class RedisCacheIntegrationTests
{
    private readonly IServiceProvider _serviceProvider;

    public RedisCacheIntegrationTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", "Host=localhost;Database=dummy" },
                { "ConnectionStrings:Redis", "localhost:6379" },
                { "Security:Jwt:Issuer", "VGS.RetailOS" },
                { "Security:Jwt:Audience", "VGS.RetailOS.App" },
                { "Security:Jwt:ExpirationMinutes", "15" },
                { "Security:Jwt:SecretKey", "vgs_dev_jwt_signing_key_min_32_characters_long_placeholder" }
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(configuration);

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void AddInfrastructure_ShouldRegister_ConnectionMultiplexer_And_RedisCacheService()
    {
        // Act
        var connectionMultiplexer = _serviceProvider.GetService<IConnectionMultiplexer>();
        var redisCache = _serviceProvider.GetService<IRedisCache>();

        // Assert
        Assert.NotNull(connectionMultiplexer);
        Assert.NotNull(redisCache);
    }

    [Fact]
    public async Task RedisCacheService_Should_SetGetDelete_Successfully()
    {
        // Arrange
        var redisCache = _serviceProvider.GetRequiredService<IRedisCache>();
        var testKey = $"test-key-{Guid.NewGuid()}";
        var testValue = "integration-test-value";

        // Act & Assert
        // 1. Check exists initially (should be false)
        var existsInitially = await redisCache.ExistsAsync(testKey);
        Assert.False(existsInitially);

        // 2. Set value
        await redisCache.SetAsync(testKey, testValue, TimeSpan.FromMinutes(1));

        // 3. Get value
        var retrievedValue = await redisCache.GetAsync(testKey);
        Assert.Equal(testValue, retrievedValue);

        // 4. Check exists after set (should be true)
        var existsAfterSet = await redisCache.ExistsAsync(testKey);
        Assert.True(existsAfterSet);

        // 5. Delete value
        await redisCache.RemoveAsync(testKey);

        // 6. Check exists after delete (should be false)
        var existsAfterDelete = await redisCache.ExistsAsync(testKey);
        Assert.False(existsAfterDelete);
    }

    [Fact]
    public void RedisConnection_ShouldBe_Reusable()
    {
        var connection1 = _serviceProvider.GetRequiredService<IConnectionMultiplexer>();
        var connection2 = _serviceProvider.GetRequiredService<IConnectionMultiplexer>();

        Assert.Same(connection1, connection2);
        Assert.True(connection1.IsConnected);
    }
}
