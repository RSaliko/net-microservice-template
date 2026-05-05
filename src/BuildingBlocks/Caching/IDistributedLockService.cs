namespace BuildingBlocks.Caching;

public interface IDistributedLockService
{
    Task<bool> AcquireLockAsync(string key, string value, TimeSpan expiration, CancellationToken cancellationToken = default);
    Task ReleaseLockAsync(string key, string value, CancellationToken cancellationToken = default);
}
