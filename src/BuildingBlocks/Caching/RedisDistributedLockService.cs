using StackExchange.Redis;

namespace BuildingBlocks.Caching;

public class RedisDistributedLockService : IDistributedLockService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public RedisDistributedLockService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = _redis.GetDatabase();
    }

    public async Task<bool> AcquireLockAsync(string key, string value, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        return await _db.LockTakeAsync(key, value, expiration).ConfigureAwait(false);
    }

    public async Task ReleaseLockAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        await _db.LockReleaseAsync(key, value).ConfigureAwait(false);
    }
}
