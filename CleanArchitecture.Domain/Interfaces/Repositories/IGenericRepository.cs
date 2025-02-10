using System.Linq.Expressions;
using CleanArchitecture.Domain.Common;

namespace CleanArchitecture.Domain.Interfaces.Repositories;

/// <summary>
/// The idea of this repository is to expose transactions and general methods to the service layer,
/// any repository which will require transactions
/// can implement this interface
/// </summary>
public interface IGenericRepository<TEntity> where TEntity : BaseEntity
{
    /// <summary>
    /// This lets you add an entity locally, without having to hit the database, and then commiting your update
    /// by calling SaveChangesAsync() and CommitTransactionAsync()
    /// </summary>
    /// <param name="entity">The entity object you need to update</param>
    /// <typeparam name="TEntity">Entity type</typeparam>
    void Add(TEntity entity);

    /// <summary>
    /// Exposes AddRange method of the DbContext, without hitting the database, similar to Add
    /// </summary>
    /// <param name="entities">List of entities to Add</param>
    /// <typeparam name="TEntity">Entity Type</typeparam>
    void AddRange(IEnumerable<TEntity> entities);

    /// <summary>
    /// This lets you update an entity locally, without having to hit the database, and then commiting your update
    /// by calling SaveChangesAsync() and CommitTransactionAsync()
    /// </summary>
    /// <param name="entity">The entity object you need to update</param>
    /// <typeparam name="TEntity">Entity type</typeparam>
    void Update(TEntity entity);

    /// <summary>
    /// Get any TEntity
    /// </summary>
    /// <typeparam name="TEntity">The entity object you need to fetch</typeparam>
    /// <returns>Entity type</returns>
    Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate, bool forUpdate = false,
        params Expression<Func<TEntity, object>>[] includes);

    /// <summary>
    /// Get any TEntity and append ForUpdate - used for pessimistic concurrency control
    /// </summary>
    /// <typeparam name="TEntity">The entity object you need to fetch</typeparam>
    /// <returns>Entity type</returns>
    Task<TEntity> GetForUpdateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, object>> orderBy = null,
        bool orderByAscending = true);

    /// <summary>
    /// Get any multiple records
    /// </summary>
    /// <typeparam name="TEntity">The entity object you need to fetch</typeparam>
    /// <returns>Entity type</returns>
    Task<List<TEntity>> GetManyAsync(Expression<Func<TEntity, bool>> predicate, bool forUpdate = false,
        params Expression<Func<TEntity, object>>[] includes);

    /// <summary>
    /// Check if any entity matching the predicate exists in the database
    /// </summary>
    /// <typeparam name="TEntity">The entity object we need to check existence of</typeparam>
    /// <returns>Entity type</returns>
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Runs an execute delete command that matches the specified predicate
    /// </summary>
    /// <typeparam name="TEntity">The entity object we need to check existence of</typeparam>
    /// <returns>Entity type</returns>
    Task ExecuteDeleteAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Mark a list of entities as deleted
    /// </summary>
    /// <param name="entities">The entities required for delete</param>
    /// <typeparam name="TEntity">Entity type</typeparam>
    void DeleteRange(IEnumerable<TEntity> entities);

    /// <summary>
    /// Mark a single entity as deleted
    /// </summary>
    /// <param name="entity">The entity required for delete</param>
    /// <typeparam name="TEntity">Entity type</typeparam>
    void Delete(TEntity entity);
}
