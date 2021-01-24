using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace IdentityServer.Senders
{
    public interface IEmailSender
    {
        Task SendAsync(EmailMessage emailMessage);
    }

    public class EmailSender : IEmailSender
    {
        readonly Options _options;

        public EmailSender(IOptions<Options> options)
        {
            _options = options.Value;
        }

        public async Task SendAsync(EmailMessage emailMessage)
        {
            var message = new MimeMessage();

            if (emailMessage.FromAddresses.Any())
                message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            else
                message.From.Add(new MailboxAddress(_options.DefaultSenderName, _options.DefaultSenderEmail));

            message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            message.Subject = emailMessage.Subject;
            message.Body = new TextPart(TextFormat.Html)
            {
                Text = emailMessage.Content
            };

            using var emailClient = new SmtpClient();

            await emailClient.ConnectAsync(_options.SmtpServer, _options.SmtpPort, false);
            await emailClient.AuthenticateAsync(_options.SmtpUsername, _options.SmtpPassword);
            await emailClient.SendAsync(message);
            await emailClient.DisconnectAsync(true);
        }

        public class Options
        {
            public string? SmtpServer { get; set; }
            public int SmtpPort { get; set; }
            public string? SmtpUsername { get; set; }
            public string? SmtpPassword { get; set; }
            public string? DefaultSenderEmail { get; set; }
            public string? DefaultSenderName { get; set; }
        }
    }
}