using System.Diagnostics;
using CleanArchitecture.Domain.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Application.Common.Behaviours;

public class PerformanceBehaviour<TRequest, TResponse>(ILogger<TRequest> logger, IConfiguration configuration)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly Stopwatch _timer = new();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        var time = Math.Max(configuration.GetSection("SystemSettings")["ResponseTimeLimit"].ToInt(), 1000);

        if (elapsedMilliseconds <= time)
            return response;

        var requestName = typeof(TRequest).Name;

        logger.LogWarning(
            "Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds)  {@Request}",
            requestName, elapsedMilliseconds, request);

        return response;
    }
}
