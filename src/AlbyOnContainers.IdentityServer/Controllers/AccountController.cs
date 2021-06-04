using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AlbyOnContainers.Messages;
using IdentityModel;
using IdentityServer;
using IdentityServer.Extensions;
using IdentityServer.Models;
using IdentityServer.Models.AccountViewModels;
using IdentityServer.Options;
using IdentityServer.Publishers;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IdentityServer.Controllers
{
    [SecurityHeaders]
    [AllowAnonymous]
    public partial class AccountController : Controller
    {
        private readonly EmailOptions _emailOptions;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailPublisher _publisher;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly TokenLifetimeOptions _tokenOptions;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IClientStore _clientStore;

        public AccountController(
            UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            IEmailPublisher publisher, IIdentityServerInteractionService interaction,
            IOptions<EmailOptions> emailOptions, IOptions<TokenLifetimeOptions> tokenOptions,
            ILogger<AccountController> logger, IClientStore clientStore)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _publisher = publisher;
            _interaction = interaction;
            _emailOptions = emailOptions.Value;
            _tokenOptions = tokenOptions.Value;
            _logger = logger;
            _clientStore = clientStore;
        }

        private async Task PublishConfirmEmailMessage(ApplicationUser user, string? returnUrl = default)
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

        private async Task PublishForgotPasswordEmailMessage(ApplicationUser user, string? returnUrl = default)
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
        
        private EmailMessage BuildEmailMessage(ApplicationUser user, string subject, string body) =>
            new()
            {
                Sender = new MailAddress {Email = _emailOptions.Email, Name = _emailOptions.Address},
                Subject = subject,
                Body = body,
                To = new[] {new MailAddress {Name = user.UserName, Email = user.Email}}
            };
    }
}