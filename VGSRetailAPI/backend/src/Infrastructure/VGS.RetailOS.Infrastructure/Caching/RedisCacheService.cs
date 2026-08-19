using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using VGS.RetailOS.Shared.BuildingBlocks.Caching;

namespace VGS.RetailOS.Infrastructure.Caching;

public class RedisCacheService : IRedisCache
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly IDatabase _database;

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisCacheService> logger)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _logger = logger;
        _database = _connectionMultiplexer.GetDatabase();
    }

    public async Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await _database.StringGetAsync(key);
            return value.HasValue ? value.ToString() : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from Redis for key {Key}", key);
            throw;
        }
    }

    public async Task SetAsync(string key, string value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (expiration.HasValue)
            {
                await _database.StringSetAsync(key, value, expiration.Value);
            }
            else
            {
                await _database.StringSetAsync(key, value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in Redis for key {Key}", key);
            throw;
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _database.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing value from Redis for key {Key}", key);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _database.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence in Redis for key {Key}", key);
            throw;
        }
    }
}
