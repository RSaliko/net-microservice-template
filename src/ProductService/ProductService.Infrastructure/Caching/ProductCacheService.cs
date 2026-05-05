using Microsoft.Extensions.Caching.Distributed;

namespace ProductService.Infrastructure.Caching;

public class ProductCacheService
{
    private readonly IDistributedCache _cache;

    public ProductCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    // Rule 118: Cache-aside pattern implementation
}
