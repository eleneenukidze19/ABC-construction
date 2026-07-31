namespace ABC_construction.Models;

/// <summary>
/// A single page of results plus the metadata needed to render pagination
/// controls. Used by the repository layer (README section 11) and passed up
/// through services to controllers.
/// </summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    public int TotalCount { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 9;

    public int TotalPages => PageSize <= 0
        ? 0
        : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPrevious => Page > 1;

    public bool HasNext => Page < TotalPages;

    /// <summary>Projects the items into another shape, preserving page metadata.</summary>
    public PagedResult<TOut> Map<TOut>(Func<T, TOut> selector) => new()
    {
        Items = Items.Select(selector).ToList(),
        TotalCount = TotalCount,
        Page = Page,
        PageSize = PageSize
    };
}
