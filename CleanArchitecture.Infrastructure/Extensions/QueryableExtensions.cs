using CleanArchitecture.Domain.Shared.Dtos;

namespace CleanArchitecture.Infrastructure.Extensions;

public static class QueryableExtensions
{
    /// <summary>
    /// This method is uses offset pagination to get a paginated result from a certain collection or queryable object
    /// </summary>
    /// <param name="source">The source list or queryable object</param>
    /// <param name="page">The requested page number</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <typeparam name="T">The entity type</typeparam>
    /// <returns>A paginated result of the entity T</returns>
    public static async Task<PagedResult<T>> ToPagedAsync<T>(this IQueryable<T> source, int page = 1,
        int pageSize = 10)
    {
        var totalCount = await source.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<T>(items, page, pageSize, totalCount, totalPages);
    }

    /// <summary>
    /// This is an extension method to implement pessimistic concurrency control in EF Core. The idea is that the
    /// .TagWith() method will cause the query's text to start with "-- ForUpdate", which will be handled by
    /// an "interceptor" which will simply append a "FOR UPDATE NOWAIT" to the query and causes it to acquire a "ROW SHARE"
    /// lock on that entity. ROW SHARE will allow reads if the other transaction is trying to read with a regular SELECT.
    /// However, if another client tries to read with a "FOR UPDATE NOWAIT" select, they will immediately get an error.
    /// Retry Policies should be used to handle that error and retry the transaction.
    /// </summary>
    /// <param name="query">This is the queryable object you need to tag with "FOR UPDATE"</param>
    /// <typeparam name="TEntity">Entity / DBSet on which you are executing the query</typeparam>
    /// <returns>A new queryable object flagged with "-- ForUpdate" tag which will be intercepted
    /// and FOR UPDATE NOWAIT will be appended to the underlying DbCommand</returns>
    public static IQueryable<TEntity> ForUpdate<TEntity>(this IQueryable<TEntity> query)
        where TEntity : class
    {
        return query.TagWith("ForUpdate");
    }
}
