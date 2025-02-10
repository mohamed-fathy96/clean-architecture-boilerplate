using System.Data;
using CleanArchitecture.Domain.Interfaces;
using CleanArchitecture.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore.Storage;

namespace CleanArchitecture.Infrastructure.Common;

public class UnitOfWork : IUnitOfWork
{
    private readonly ReadDbContext _readDbContext;
    private readonly WriteDbContext _writeDbContext;
    private IDbContextTransaction _currentTransaction;

    public UnitOfWork(ReadDbContext readDbContext, WriteDbContext writeDbContext)
    {
        _readDbContext = readDbContext;
        _writeDbContext = writeDbContext;
        InitializeRepositories();
    }
    
    public async Task BeginTransactionAsync(IsolationLevel isolationLevel)
    {
        _currentTransaction ??= await _writeDbContext.Database.BeginTransactionAsync(isolationLevel);
    }

    public async Task CommitTransactionAsync()
    {
        if (_currentTransaction is not null)
        {
            await _currentTransaction.CommitAsync();
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_currentTransaction is not null)
        {
            await _currentTransaction.RollbackAsync();
        }

        await _writeDbContext.Database.RollbackTransactionAsync();
        Refresh();
    }

    public async Task SaveChangesAsync()
    {
        await _writeDbContext.SaveChangesAsync();
    }

    /// <summary>
    /// When a transaction fails due to concurrent updates, the next time you retry to fetch the data,
    /// you're using the same DbContext instance - the result is you get stale data, this method will detach
    /// the entries in the context to ensure when you retry you get the latest instance of the data
    /// </summary>
    private void Refresh()
    {
        var changedEntries = _writeDbContext.ChangeTracker.Entries().ToList();
        foreach (var entry in changedEntries)
        {
            entry.State = EntityState.Detached;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_readDbContext != null) await _readDbContext.DisposeAsync();
        if (_writeDbContext != null) await _writeDbContext.DisposeAsync();
        if (_currentTransaction != null) await _currentTransaction.DisposeAsync();
    }

    public void Dispose()
    {
        _readDbContext?.Dispose();
        _writeDbContext?.Dispose();
        _currentTransaction?.Dispose();
    }

    private void InitializeRepositories()
    {
    }
}
