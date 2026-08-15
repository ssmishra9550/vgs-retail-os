using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace VGS.RetailOS.Infrastructure.HealthChecks;

public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisHealthCheck(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _connectionMultiplexer.GetDatabase();
            var ping = await db.PingAsync();
            return HealthCheckResult.Healthy($"Redis is reachable. Ping time: {ping.TotalMilliseconds}ms.");
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, "Redis connection failed.", ex); // Exception is passed to the result, but won't be exposed by the writer
        }
    }
}
