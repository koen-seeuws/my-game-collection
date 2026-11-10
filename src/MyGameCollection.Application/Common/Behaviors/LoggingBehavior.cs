using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MyGameCollection.Application.Common.Behaviors;

/// <summary>Logs every request and warns about slow ones.</summary>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly TimeSpan SlowThreshold = TimeSpan.FromMilliseconds(750);

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var name = typeof(TRequest).Name;
        var started = Stopwatch.GetTimestamp();
        try
        {
            return await next(cancellationToken);
        }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(started);
            if (elapsed > SlowThreshold)
                logger.LogWarning("{Request} took {Elapsed:N0} ms", name, elapsed.TotalMilliseconds);
            else
                logger.LogDebug("{Request} handled in {Elapsed:N0} ms", name, elapsed.TotalMilliseconds);
        }
    }
}
