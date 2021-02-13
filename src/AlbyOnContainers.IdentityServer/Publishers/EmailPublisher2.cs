using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using AlbyOnContainers.Messages;
using IdentityServer.Options;
using MassTransit;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IdentityServer.Publishers
{
    public interface IEmailPublisher2
    {
        Task SendConfirmationEmailAsync(string id, string code, string host, string returnUrl, string user, string email, CancellationToken cancellationToken = default);
        Task SendConfirmationChangeEmailAsync(string id, string code, string host, string returnUrl, string user, string email, CancellationToken cancellationToken = default);

        Task SendRecoverPasswordEmailAsync(string id, string code, string host, string returnUrl, string user, string email, CancellationToken cancellationToken = default);
    }

    public sealed class EmailPublisher2 : EmailPublisher, IEmailPublisher2
    {
        readonly EmailOptions _options;

        public EmailPublisher2(IOptions<EmailOptions> options, IPublishEndpoint publishEndpoint, ILogger<EmailPublisher> logger) : base(publishEndpoint, logger)
        {
            _options = options.Value;
        }

        public async Task SendConfirmationEmailAsync(string id, string code, string host, string returnUrl, string user, string email, CancellationToken cancellationToken = default)
        {
            var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var param = new Dictionary<string, string> { { "code", encoded }, { "userId", id }, { "returnUrl", returnUrl } };

            var callbackUrl = QueryHelpers.AddQueryString(host, param);

            var message = new EmailMessage
            {
                Sender = new MailAddress { Email = _options.Email, Name = _options.Name },
                Subject = "Conferma la tua email",
                Body = $"Ciao {user}, <br /> Per confermare la tua email <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicca qui!</a>.",
                To = new[] { new MailAddress { Name = user, Email = email } }
            };

            await SendAsync(message, cancellationToken);
        }

        public async Task SendConfirmationChangeEmailAsync(string id, string code, string host, string returnUrl, string user, string email, CancellationToken cancellationToken = default)
        {
            var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var param = new Dictionary<string, string> { { "code", encoded }, { "userId", id }, {"email", email}, { "returnUrl", returnUrl } };

            var callbackUrl = QueryHelpers.AddQueryString(host, param);

            var message = new EmailMessage
            {
                Sender = new MailAddress { Email = _options.Email, Name = _options.Name },
                Subject = "Conferma nuova email",
                Body = $"Ciao {user}, <br /> Per confermare il tuo account <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicca qui!</a>.",
                To = new[] { new MailAddress { Name = user, Email = email } }
            };

            await SendAsync(message, cancellationToken);
        }

        public async Task SendRecoverPasswordEmailAsync(string id, string code, string host, string returnUrl, string user, string email, CancellationToken cancellationToken = default)
        {
            var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var param = new Dictionary<string, string> { { "code", encoded }, { "returnUrl", returnUrl } };

            var callbackUrl = QueryHelpers.AddQueryString(host, param);

            var message = new EmailMessage
            {
                Sender = new MailAddress { Email = _options.Email, Name = _options.Name },
                Subject = "Recupero password",
                Body = $"Ciao {user}, <br /> Per recuperare la tua password <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicca qui!</a>.",
                To = new[] { new MailAddress { Name = user, Email = email } }
            };

            await SendAsync(message, cancellationToken);
        }
    }
}