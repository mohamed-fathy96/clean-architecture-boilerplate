using System.Linq.Expressions;
using CleanArchitecture.Domain.Common;
using CleanArchitecture.Domain.Interfaces.Repositories;
using CleanArchitecture.Infrastructure.Contexts;
using CleanArchitecture.Infrastructure.Extensions;

namespace CleanArchitecture.Infrastructure.Repositories;

internal class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly ReadDbContext ReadDbContext;
    protected readonly WriteDbContext WriteDbContext;

    public GenericRepository(ReadDbContext readDbContext, WriteDbContext writeDbContext)
    {
        ReadDbContext = readDbContext;
        WriteDbContext = writeDbContext;
    }

    public virtual void Add(TEntity entity)
    {
        WriteDbContext.Set<TEntity>().Add(entity);
    }

    public virtual void AddRange(IEnumerable<TEntity> entities)
    {
        WriteDbContext.Set<TEntity>().AddRange(entities);
    }

    public virtual void Update(TEntity entity)
    {
        WriteDbContext.Set<TEntity>().Update(entity);
    }

    public virtual async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate, bool forUpdate = false,
        params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = forUpdate
            ? WriteDbContext.Set<TEntity>()
            : ReadDbContext.Set<TEntity>();

        query = includes.Aggregate(query, (current, include) => current.Include(include));

        return await query.FirstOrDefaultAsync(predicate);
    }

    public async Task<TEntity> GetForUpdateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, object>> orderBy = null,
        bool orderByAscending = true)
    {
        var query = WriteDbContext
            .Set<TEntity>()
            .AsQueryable();

        if (orderBy != null)
        {
            query = orderByAscending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
        }

        return await query
            .ForUpdate()
            .FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<List<TEntity>> GetManyAsync(Expression<Func<TEntity, bool>> predicate,
        bool forUpdate = false, params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = forUpdate
            ? WriteDbContext.Set<TEntity>()
            : ReadDbContext.Set<TEntity>();

        query = includes.Aggregate(query, (current, include) => current.Include(include));

        return await query
            .Where(predicate)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await WriteDbContext
            .Set<TEntity>()
            .Where(predicate)
            .AnyAsync();
    }

    public async Task ExecuteDeleteAsync(Expression<Func<TEntity, bool>> predicate)
    {
        await WriteDbContext
            .Set<TEntity>()
            .Where(predicate)
            .ExecuteDeleteAsync();
    }

    public virtual void DeleteRange(IEnumerable<TEntity> entities)
    {
        WriteDbContext.Set<TEntity>().RemoveRange(entities);
    }

    public virtual void Delete(TEntity entity)
    {
        WriteDbContext.Set<TEntity>().Remove(entity);
    }
}
