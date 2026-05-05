namespace BuildingBlocks.Models;

/// <summary>
/// Standard wrapper for paginated data.
/// </summary>
/// <typeparam name="T">The type of data being paginated.</typeparam>
public record PaginatedResult<T>(
    IReadOnlyCollection<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount
)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
