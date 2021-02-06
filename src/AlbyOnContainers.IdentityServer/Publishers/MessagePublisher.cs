using System;
using System.Threading;
using System.Threading.Tasks;
using AlbyOnContainers.Messages;
using IdentityServer.Abstract;
using MassTransit;
using Microsoft.Extensions.Logging;
using Polly;

namespace IdentityServer.Publishers
{
    public class MessagePublisher : IMessagePublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<MessagePublisher> _logger;

        public MessagePublisher(IPublishEndpoint publishEndpoint, ILogger<MessagePublisher> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }
        
        public async Task Send(CancellationToken cancellationToken, EmailMessage message)
        {
            const int retries = 3;
            var retry = Policy.Handle<Exception>()
                .WaitAndRetryAsync(retries,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, _, r, _) => _logger.LogWarning(exception, "[{prefix}] Exception {ExceptionType} with message {Message} detected on attempt {retry} of {retries}", nameof(Program), exception.GetType().Name,
                        exception.Message, r, retries)
                );

            await retry.ExecuteAsync(async () => await _publishEndpoint.Publish(message, cancellationToken));
        }
    }
}