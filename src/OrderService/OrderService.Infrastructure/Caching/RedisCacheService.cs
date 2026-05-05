using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace OrderService.Infrastructure.Caching;

public class RedisCacheService
{
    private readonly IDistributedCache _cache;

    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
    {
        var data = await _cache.GetStringAsync(key, cancellationToken);
        return data == null ? default : JsonConvert.DeserializeObject<T>(data);
    }
}
