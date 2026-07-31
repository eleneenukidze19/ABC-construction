using System.Collections.Concurrent;
using ABC_construction.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace ABC_construction.Services;

/// <inheritdoc cref="ICacheService"/>
/// <remarks>
/// Backed by <see cref="IMemoryCache"/>. Region membership is tracked
/// separately because IMemoryCache cannot enumerate or remove keys by prefix.
/// </remarks>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCacheService> _logger;

    /// <summary>region -> set of cache keys currently belonging to it.</summary>
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> RegionKeys = new();

    /// <summary>
    /// Serialises factory execution per key so a burst of concurrent misses on
    /// a cold cache results in one database round-trip, not one per request.
    /// </summary>
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> KeyLocks = new();

    public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T> GetOrCreateAsync<T>(
        string region,
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan absoluteExpiration,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildKey(region, key);

        if (_cache.TryGetValue(cacheKey, out T? cached) && cached is not null)
        {
            return cached;
        }

        var gate = KeyLocks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);

        try
        {
            // Re-check: another request may have populated it while we waited.
            if (_cache.TryGetValue(cacheKey, out cached) && cached is not null)
            {
                return cached;
            }

            var value = await factory(cancellationToken);

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration
            };

            // Keep the region index in step when an entry expires or is evicted
            // under memory pressure, otherwise the index grows without bound.
            options.RegisterPostEvictionCallback((evictedKey, _, _, _) =>
            {
                if (RegionKeys.TryGetValue(region, out var keys))
                {
                    keys.TryRemove(evictedKey.ToString()!, out _);
                }

                KeyLocks.TryRemove(evictedKey.ToString()!, out _);
            });

            _cache.Set(cacheKey, value, options);
            RegionKeys.GetOrAdd(region, _ => new ConcurrentDictionary<string, byte>())[cacheKey] = 0;

            return value;
        }
        finally
        {
            gate.Release();
        }
    }

    public void RemoveRegion(string region)
    {
        if (!RegionKeys.TryRemove(region, out var keys))
        {
            return;
        }

        foreach (var key in keys.Keys)
        {
            _cache.Remove(key);
            KeyLocks.TryRemove(key, out _);
        }

        _logger.LogInformation(
            "Cache region {Region} invalidated ({Count} entries removed).", region, keys.Count);
    }

    private static string BuildKey(string region, string key) => $"{region}:{key}";
}
