using MediatR;
using Microsoft.Extensions.Logging;
using OrdersService.Application.Services.Interfaces;

namespace OrdersService.Application.PipelineBehaviours;

public class LoggingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingPipelineBehavior<TRequest, TResponse>> _logger;
    private readonly IUserAccessor _userAccessor;

    public LoggingPipelineBehavior(IUserAccessor userAccessor, ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger)
    {
        _userAccessor = userAccessor;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        _logger.LogInformation("Handling {CommandName}, initiated by {UserName}.", typeof(TRequest).Name, _userAccessor.Current.Name);
        TResponse response = await next();
        _logger.LogInformation("Handled {CommandName}, initiated by {UserName}.", typeof(TRequest).Name, _userAccessor.Current.Name);
        return response;
    }
}