using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CleanArchitecture.Infrastructure.Interceptors;

/// <summary>
/// This is the interceptor which is responsible for detecting DbCommands which has the "-- For Update" tag and append
/// a for update to the command's text to acquire a Row Share lock on that row
/// </summary>
public class ForUpdateInterceptor : DbCommandInterceptor
{
    public override InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData,
        InterceptionResult<object> result)
    {
        ManipulateCommand(command);
        return result;
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command,
        CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default)
    {
        ManipulateCommand(command);
        return new(result);
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        ManipulateCommand(command);
        return result;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
        CommandEventData eventData, InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        ManipulateCommand(command);
        return new(result);
    }

    private static void ManipulateCommand(IDbCommand command)
    {
        if (command.CommandText.StartsWith("-- ForUpdate", StringComparison.Ordinal))
        {
            command.CommandText += " FOR UPDATE NOWAIT";
        }
    }
}
