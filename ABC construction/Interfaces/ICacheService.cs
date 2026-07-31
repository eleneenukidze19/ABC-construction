namespace ABC_construction.Interfaces;

/// <summary>
/// Read-through cache with region-based invalidation (README section 17).
/// <para>
/// Regions exist because a single logical dataset spans many cache keys — the
/// projects list alone is keyed by page, page size and category. When an admin
/// edits one project, every one of those keys is potentially stale, and
/// IMemoryCache offers no way to enumerate or remove by prefix. Callers
/// therefore drop the whole region rather than guessing key names.
/// </para>
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Returns the cached value for <paramref name="key"/>, invoking
    /// <paramref name="factory"/> and caching its result on a miss.
    /// </summary>
    Task<T> GetOrCreateAsync<T>(
        string region,
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan absoluteExpiration,
        CancellationToken cancellationToken = default);

    /// <summary>Evicts every entry in a region. Called after admin writes.</summary>
    void RemoveRegion(string region);
}

/// <summary>Cache region names, kept in one place to avoid typo-driven stale reads.</summary>
public static class CacheRegions
{
    public const string Projects = "projects";
    public const string Employees = "employees";
    public const string Company = "company";
}
