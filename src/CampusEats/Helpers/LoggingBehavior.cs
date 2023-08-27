using CampusEats.Core.Common;
using CampusEats.Core.Identity.Application.Services;
using MediatR;

namespace CampusEats.Helpers;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUser _currentUser;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger, ICurrentUser currentUser)
    {
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var email = string.IsNullOrWhiteSpace(_currentUser.Email) ? "Anonymous" : _currentUser.Email;

        TResponse response;
        _logger.LogInformation("{email} sent request {@Request} at {time}", email, request, DateTime.Now);
        try
        {
            response = await next();
            _logger.LogInformation("{email} sent request got response {@Response} at {time}", email, response, DateTime.Now);
        }
        catch(BaseException ex)
        {
            _logger.LogError("Request sent by {email} failed at {time}: {type} {message}", email, DateTime.Now, ex.GetType().Name, ex.Message);
            throw;
        }
        return response;
    }
}