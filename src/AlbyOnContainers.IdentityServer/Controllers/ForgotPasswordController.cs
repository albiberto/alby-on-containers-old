using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AlbyOnContainers.Messages;
using IdentityServer.Models;
using IdentityServer.Options;
using IdentityServer.Publishers;
using IdentityServer.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace IdentityServer.Controllers
{
    public class ForgotPasswordController : Controller
    {
        readonly UserManager<ApplicationUser> _userManager;
        readonly IEmailPublisher _publisher;
        readonly EmailOptions _options;

        public ForgotPasswordController(UserManager<ApplicationUser> userManager, IEmailPublisher publisher, IOptions<EmailOptions> options)
        {
            _userManager = userManager;
            _publisher = publisher;
            _options = options.Value;
        }

        [HttpGet, AllowAnonymous]
        public IActionResult Index(string? returnUrl = default)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ForgotPasswordViewModel model, string? returnUrl = default)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            
            // Don't reveal that the user does not exist or is not confirmed
            if (user == default || !await _userManager.IsEmailConfirmedAsync(user)) return View("ForgotPasswordConfirmation", model.Email);

            await PublishForgotPasswordEmailMessage(user, returnUrl);

            return View("ForgotPasswordConfirmation", model.Email);
        }
        
        async Task PublishForgotPasswordEmailMessage(ApplicationUser user, string? returnUrl = default)
        {
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            
            var callbackUrl = Url.Action("Index", "ResetPassword", new {code, returnUrl}, Request.Scheme);
            if(callbackUrl == default) return;
            
            const string subject = "Recupero Password";
            string body = $"Ciao {user.UserName}, <br /> Per recuperare la tua password <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicca qui!</a>.";

            var message = BuildEmailMessage(user, subject, body);
            await _publisher.SendAsync(message);
        }

        EmailMessage BuildEmailMessage(ApplicationUser user, string subject, string body) =>
            new()
            {
                Sender = new MailAddress {Email = _options.Email, Name = _options.Address},
                Subject = subject,
                Body = body,
                To = new[] {new MailAddress {Name = user.UserName, Email = user.Email}}
            };
    }
}