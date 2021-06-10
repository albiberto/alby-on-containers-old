using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AlbyOnContainers.Messages;
using IdentityServer.Models;
using IdentityServer.Options;
using IdentityServer.Publishers;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IdentityServer.Controllers
{
    [AllowAnonymous]
    public partial class AccountController : Controller
    {
        readonly EmailOptions _emailOptions;
        readonly IIdentityServerInteractionService _interaction;
        readonly ILogger<AccountController> _logger;
        readonly IEmailPublisher _publisher;
        readonly SignInManager<ApplicationUser> _signInManager;
        readonly UserManager<ApplicationUser> _userManager;
        readonly IEventService _events;


        public AccountController(
            UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            IEmailPublisher publisher, IIdentityServerInteractionService interaction,
            IOptions<EmailOptions> emailOptions,
            ILogger<AccountController> logger, IEventService events)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _publisher = publisher;
            _interaction = interaction;
            _emailOptions = emailOptions.Value;
            _logger = logger;
            _events = events;
        }

        async Task PublishConfirmEmailMessage(ApplicationUser user, string? returnUrl = default)
        {
            var userId = await _userManager.GetUserIdAsync(user);

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Action("ConfirmEmail", "Account", new {userId, code, returnUrl}, Request.Scheme);
            if(callbackUrl == default) return;

            const string subject = "Conferma Email";
            string body = $"Ciao {user.UserName}, <br /> Per confermare il tuo account <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicca qui!</a>.";
            
            var message = BuildEmailMessage(user, subject, body);
            await _publisher.SendAsync(message);
        }

        async Task PublishForgotPasswordEmailMessage(ApplicationUser user, string? returnUrl = default)
        {
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            
            var callbackUrl = Url.Action("ResetPassword", "Account", new {code, returnUrl}, Request.Scheme);
            if(callbackUrl == default) return;
            
            const string subject = "Recupero Password";
            string body = $"Ciao {user.UserName}, <br /> Per recuperare la tua password <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicca qui!</a>.";

            var message = BuildEmailMessage(user, subject, body);
            await _publisher.SendAsync(message);
        }

        EmailMessage BuildEmailMessage(ApplicationUser user, string subject, string body) =>
            new()
            {
                Sender = new MailAddress {Email = _emailOptions.Email, Name = _emailOptions.Address},
                Subject = subject,
                Body = body,
                To = new[] {new MailAddress {Name = user.UserName, Email = user.Email}}
            };
    }
}