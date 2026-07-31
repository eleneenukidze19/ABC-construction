namespace ABC_construction.DTOs;

/// <summary>
/// Envelope for paged API responses, keeping page metadata out of the item
/// shape so clients can render pagination controls.
/// </summary>
public class PagedResponse<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasPrevious { get; init; }
    public bool HasNext { get; init; }
}
