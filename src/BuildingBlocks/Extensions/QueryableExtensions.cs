using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Models;

namespace BuildingBlocks.Extensions;

public static class QueryableExtensions
{
    /// <summary>
    /// Extension method to paginate an IQueryable source into a PaginatedResult.
    /// </summary>
    public static async Task<PaginatedResult<T>> ToPaginatedResultAsync<T>(
        this IQueryable<T> source, 
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var count = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<T>(items, pageNumber, pageSize, count);
    }
}
