using System.Linq.Expressions;

namespace BuildingBlocks.Data;

/// <summary>
/// BP #34: Read-Only Repository for Read-Write Splitting (Replica access).
/// </summary>
public interface IReadOnlyRepository<T, in TId> where T : class
{
    Task<T?> FindByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    IQueryable<T> Query();
}
