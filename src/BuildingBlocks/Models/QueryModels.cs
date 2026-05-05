namespace BuildingBlocks.Models;

public record PagingParams(int PageNumber = 1, int PageSize = 10);

public record PagedResult<T>(IEnumerable<T> Items, int TotalCount, int PageNumber, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

public record SortingParams(string? SortBy = null, bool IsDescending = false);

public record FilteringParams(string? SearchTerm = null, Dictionary<string, string>? Filters = null);
