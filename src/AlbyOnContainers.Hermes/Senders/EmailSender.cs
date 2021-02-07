using System.Linq;
using System.Threading.Tasks;
using AlbyOnContainers.Messages;
using Hermes.Abstract;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Hermes.Senders
{
    public class EmailSender : ISender<EmailMessage>
    {
        readonly ILogger<EmailSender> _logger;
        readonly Options _options;

        public EmailSender(IOptions<Options> options, ILogger<EmailSender> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendAsync(EmailMessage message)
        {
            var mailMessage = new MimeMessage();

            var bodyBuilder = new BodyBuilder {HtmlBody = $"<p>{message.Body}</p>"};

            mailMessage.From.Add(new MailboxAddress(_options.DefaultSenderName, _options.DefaultSenderEmail));
            mailMessage.To.AddRange(message.To.Select(m => new MailboxAddress(m.Name, m.Email)));
            mailMessage.Subject = message.Subject;
            mailMessage.Body = bodyBuilder.ToMessageBody();

            using var smtpClient = new SmtpClient
            {
                ServerCertificateValidationCallback = (_, __, ___, ____) => true
            };

            await smtpClient.ConnectAsync(_options.SmtpServer, _options.SmtpPort, _options.UseSsl);
            await smtpClient.AuthenticateAsync(_options.SmtpUsername, _options.SmtpPassword);

            await smtpClient.SendAsync(mailMessage);

            await smtpClient.DisconnectAsync(true);
        }

        public class Options
        {
            public string SmtpServer { get; set; }
            public int SmtpPort { get; set; }
            public string SmtpUsername { get; set; }
            public string SmtpPassword { get; set; }
            public string DefaultSenderName { get; set; }
            public string DefaultSenderEmail { get; set; }
            public bool UseSsl { get; set; }
        }
    }
}