using System;
using System.Threading;
using System.Threading.Tasks;
using AlbyOnContainers.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using Polly;

namespace IdentityServer.Publishers
{
    public abstract class PublisherBase
    {
        readonly IPublishEndpoint _publishEndpoint;
        readonly ILogger<PublisherBase> _logger;

        protected PublisherBase(IPublishEndpoint publishEndpoint, ILogger<PublisherBase> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        protected  virtual async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            const int retries = 3;

            var retry = Policy.Handle<Exception>()
                .WaitAndRetryAsync(retries,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(x: 2, retryAttempt)),
                    (exception, _, r, _) => _logger.LogWarning(exception, "[{prefix}] Exception {ExceptionType} with message {Message} detected on attempt {retry} of {retries}", nameof(Program), exception.GetType().Name,
                        exception.Message, r, retries)
                );

            await retry.ExecuteAsync(async () => await _publishEndpoint.Publish(message, cancellationToken));
        }
    }
}