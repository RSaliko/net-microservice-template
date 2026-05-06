using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json.Serialization;

namespace BuildingBlocks.Caching;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}

public class CacheService(IDistributedCache cache) : ICacheService
{
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var cachedValue = await cache.GetStringAsync(key, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrEmpty(cachedValue))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(cachedValue);
        }
        catch (JsonException)
        {
            // Cache corrupted, return null to force fresh fetch
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(10)
        };

        var serializedValue = JsonSerializer.Serialize(value);
        await cache.SetStringAsync(key, serializedValue, options, cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await cache.RemoveAsync(key, cancellationToken).ConfigureAwait(false);
    }
}
