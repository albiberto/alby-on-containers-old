using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AlbyOnContainers.Messages;
using IdentityServer.Models;
using IdentityServer.Options;
using IdentityServer.Publishers;
using IdentityServer.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace IdentityServer.Controllers
{
    public class RegisterController : Controller
    {
        readonly UserManager<ApplicationUser> _userManager;
        readonly IEmailPublisher _publisher;
        readonly EmailOptions _emailOptions;


        public RegisterController(UserManager<ApplicationUser> userManager, IEmailPublisher publisher, IOptions<EmailOptions> emailOptions)
        {
            _userManager = userManager;
            _publisher = publisher;
            _emailOptions = emailOptions.Value;
        }

        [HttpGet]
        public IActionResult Index(string? returnUrl = default)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RegisterViewModel model, string? returnUrl = default)
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
        
        async Task PublishConfirmEmailMessage(ApplicationUser user, string? returnUrl = default)
        {
            var userId = await _userManager.GetUserIdAsync(user);

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Action("Index", "ConfirmEmail", new {userId, code, returnUrl}, Request.Scheme);
            if(callbackUrl == default) return;

            const string subject = "Conferma Email";
            string body = $"Ciao {user.UserName}, <br /> Per confermare il tuo account <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicca qui!</a>.";
            
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