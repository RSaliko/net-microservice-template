using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BuildingBlocks.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[START] Handle request={RequestName} request={RequestData}", typeof(TRequest).Name, request);
        var timer = Stopwatch.StartNew();

        var response = await next();

        timer.Stop();
        _logger.LogInformation("[END] Handled request={RequestName} ; Elapsed={Elapsed}ms", typeof(TRequest).Name, timer.ElapsedMilliseconds);

        return response;
    }
}
