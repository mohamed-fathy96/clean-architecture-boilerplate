using System.Data;

namespace CleanArchitecture.Domain.Interfaces;

public interface IUnitOfWork : IAsyncDisposable, IDisposable
{
    Task BeginTransactionAsync(IsolationLevel isolationLevel);
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task SaveChangesAsync();
}
