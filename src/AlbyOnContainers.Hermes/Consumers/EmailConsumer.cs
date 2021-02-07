using System.Threading.Tasks;
using AlbyOnContainers.Messages;
using Hermes.Abstract;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Hermes.Consumers
{
    public class EmailConsumer : IConsumer<EmailMessage>
    {
        readonly ISender<EmailMessage> _sender;
        readonly ILogger<EmailConsumer> _logger;

        public EmailConsumer(ISender<EmailMessage> sender, ILogger<EmailConsumer> logger)
        {
            _sender = sender;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<EmailMessage> context)
        {
            
            var message = context.Message;
            await _sender.SendAsync(message);
        }
    }
}