using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AlbyOnContainers.Messages;
using IdentityServer.Models;
using IdentityServer.Options;
using IdentityServer.Publishers;
using IdentityServer.ViewModels;
using IdentityServer.ViewModels.Passwords;
using IdentityServer.ViewModels.Register;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace IdentityServer.Controllers.Passwords
{
    public class PasswordsController : Controller
    {
        readonly UserManager<ApplicationUser> _userManager;
        readonly IEmailPublisher _publisher;
        readonly EmailOptions _options;

        public PasswordsController(UserManager<ApplicationUser> userManager, IEmailPublisher publisher, IOptions<EmailOptions> options)
        {
            _userManager = userManager;
            _publisher = publisher;
            _options = options.Value;
        }
        
        [HttpGet, AllowAnonymous]
        public IActionResult ForgotPassword(string? returnUrl = default)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, string? returnUrl = default)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            
            // Don't reveal that the user does not exist or is not confirmed
            if (user == default || !await _userManager.IsEmailConfirmedAsync(user)) return View("ForgotPasswordConfirmation", model.Email);

            await PublishForgotPasswordEmailMessage(user, returnUrl);

            return View("ForgotPasswordConfirmation", model.Email);
        }

        [HttpGet]
        public IActionResult ResetPassword(string? code = default, string? returnUrl = default)
        {
            if (string.IsNullOrEmpty(code)) return BadRequest("A code must be supplied for password reset.");

            ViewData["ReturnUrl"] = returnUrl;
            return View(new ResetPasswordViewModel
            {
                Code = code
            });
        }
        
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, string? returnUrl = default)
        {
            if (!ModelState.IsValid) return View();

            // Don't reveal that the user does not exist
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return View("ResetPasswordConfirmation");

            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));
            var result = await _userManager.ResetPasswordAsync(user, code, model.Password);
            
            if (result.Succeeded) return View("ResetPasswordConfirmation");

            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            return View();
        }
        
        async Task PublishForgotPasswordEmailMessage(ApplicationUser user, string? returnUrl = default)
        {
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            
            var callbackUrl = Url.Action("ResetPassword", "Passwords", new {code, returnUrl}, Request.Scheme);
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