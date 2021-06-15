using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AlbyOnContainers.Messages;
using IdentityServer.Models;
using IdentityServer.Options;
using IdentityServer.Publishers;
using IdentityServer.ViewModels;
using IdentityServer.ViewModels.Register;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace IdentityServer.Controllers.Register
{
    public class RegisterController : Controller
    {
        readonly EmailOptions _emailOptions;
        readonly IEmailPublisher _publisher;
        readonly UserManager<ApplicationUser> _userManager;


        public RegisterController(UserManager<ApplicationUser> userManager, IEmailPublisher publisher, IOptions<EmailOptions> emailOptions)
        {
            _userManager = userManager;
            _publisher = publisher;
            _emailOptions = emailOptions.Value;
        }

        #region register

        [HttpGet]
        public IActionResult Register(string? returnUrl = default)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = default)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser
            {
                Name = $"{model.GivenName} {model.FamilyName}",
                GivenName = model.GivenName,
                FamilyName = model.FamilyName,
                UserName = model.Username ?? model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
                return View();
            }

            await PublishConfirmEmailMessage(user, returnUrl);

            return View("RegisterConfirmation", model.Email);
        }

        #endregion

        #region resend email

        [HttpGet]
        public IActionResult ResendEmail(string? returnUrl = default)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResendEmail(ResendConfirmationEmailViewModel model, string? returnUrl = default)
        {
            if (!ModelState.IsValid) return ResendEmail();

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != default) await PublishConfirmEmailMessage(user, returnUrl);

            return View("ResendEmailConfirmation", model.Email);
        }

        #endregion
        
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string? userId, string? code, string? returnUrl = default)
        {
            ViewData["ReturnRul"] = returnUrl;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code)) return Redirect("~/");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == default) return NotFound($"Unable to load user with ID '{userId}'.");

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            
            var result = await _userManager.ConfirmEmailAsync(user, code);

            ViewData["StatusMessage"] = result.Succeeded
                ? "Grazie per aver confermato la tua email."
                : "Ops... si e' verificato un errore durante la consegna della mail.";
            
            return View();
        }
        
        async Task PublishConfirmEmailMessage(ApplicationUser user, string? returnUrl = default)
        {
            var userId = await _userManager.GetUserIdAsync(user);

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Action("ConfirmEmail", "Register", new {userId, code, returnUrl}, Request.Scheme);
            if (callbackUrl == default) return;

            const string subject = "Conferma Email";
            string body = $"Ciao {user.UserName}, <br /> Per confermare il tuo account <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicca qui!</a>.";

            var message = BuildEmailMessage(user, subject, body);
            await _publisher.SendAsync(message);
        }

        EmailMessage BuildEmailMessage(ApplicationUser user, string subject, string body) =>
            new()
            {
                Sender = new MailAddress {Email = _emailOptions.Name, Name = _emailOptions.Address},
                Subject = subject,
                Body = body,
                To = new[] {new MailAddress {Name = user.UserName, Email = user.Email}}
            };
    }
}