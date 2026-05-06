using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using System.Collections.Concurrent;

namespace BuildingBlocks.Caching;

/// <summary>
/// BP #15: Distributed Lock using RedLock algorithm for cross-instance locking.
/// </summary>
public class RedLockDistributedLockService : IDistributedLockService, IDisposable
{
    private readonly RedLockFactory _redLockFactory;
    private readonly ConcurrentDictionary<string, IRedLock> _activeLocks = new();

    public RedLockDistributedLockService(IConnectionMultiplexer connectionMultiplexer)
    {
        var endpoints = new List<RedLockEndPoint>();
        foreach (var endpoint in connectionMultiplexer.GetEndPoints())
        {
            endpoints.Add(new RedLockEndPoint(endpoint));
        }
        _redLockFactory = RedLockFactory.Create(endpoints);
    }

    public async Task<bool> AcquireLockAsync(string key, string value, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        var redLock = await _redLockFactory.CreateLockAsync(key, expiration).ConfigureAwait(false);
        if (redLock.IsAcquired)
        {
            _activeLocks.TryAdd($"{key}:{value}", redLock);
            return true;
        }
        return false;
    }

    public async Task ReleaseLockAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        var lockKey = $"{key}:{value}";
        if (_activeLocks.TryRemove(lockKey, out var redLock))
        {
            await redLock.DisposeAsync().ConfigureAwait(false);
        }
    }

    public void Dispose()
    {
        foreach (var redLock in _activeLocks.Values)
        {
            redLock.Dispose();
        }
        _activeLocks.Clear();
        _redLockFactory.Dispose();
    }
}
