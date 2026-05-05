using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Data;

public class Repository<T, TId> : IRepository<T, TId>
    where T : class
{
    protected readonly DbContext _dbContext;
    private readonly DbSet<T> _dbSet;

    public Repository(DbContext dbContextParam)
    {
        _dbContext = dbContextParam ?? throw new ArgumentNullException(nameof(dbContextParam));
        _dbSet = _dbContext.Set<T>();
    }

    public async Task<T?> FindByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entry = await _dbSet.FindAsync(new object?[] { id }, cancellationToken);
        return entry as T;
    }

    public Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public IQueryable<T> Query()
    {
        return _dbSet.AsQueryable();
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        await Task.CompletedTask;
    }

    public async Task DeleteByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await FindByIdAsync(id, cancellationToken);
        if (entity is not null)
        {
            await DeleteAsync(entity, cancellationToken);
        }
    }
}
