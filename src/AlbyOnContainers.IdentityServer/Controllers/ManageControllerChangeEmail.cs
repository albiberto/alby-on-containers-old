using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AlbyOnContainers.Messages;
using IdentityServer.Models.ManageViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace IdentityServer.Controllers
{
    public partial class ManageController
    {
        [HttpGet]
        public async Task<IActionResult> ChangeEmail()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == default) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            return View(new ChangeEmailViewModel
            {
                Email = user.Email,
                IsEmailConfirmed = user.EmailConfirmed,
                NewEmail = string.Empty
            });
        }

        [HttpPost]
        public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == default) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            if (!ModelState.IsValid) return View(model);

            if (string.Equals(model.Email, model.NewEmail, StringComparison.InvariantCultureIgnoreCase))
            {
                ViewData["StatusMessage"] = "L'indirizzo inserito e' uguale al precedente!";
                return View(model);
            }
            
            var code = await _userManager.GenerateChangeEmailTokenAsync(user, model.NewEmail);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Action(
                "ChangeEmailConfirmation",
                "Manage",
                new {userId = user.Id, email = model.NewEmail, code},
                Request.Scheme);

            var message = new EmailMessage
            {
                Sender = new MailAddress {Email = _options.Email, Name = _options.Address},
                Subject = "Confermi la modifica della Email?",
                Body = $"Ciao {user}, <br /> Per confermare il cambio di email <a href='{HtmlEncoder.Default.Encode(callbackUrl ?? string.Empty)}'>clicca qui!</a>.",
                To = new[] {new MailAddress {Name = user.Name, Email = model.NewEmail}}
            };

            await _email.SendAsync(message);

            ViewData["StatusMessage"] = "Ti abbiamo inviato una mail di conferma. Controlla la tua casella di posta elettronica.";
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SendVerificationEmailAsync(string email)
        {
            var model = new ChangeEmailViewModel
            {
                Email = email,
                NewEmail = string.Empty,
                IsEmailConfirmed = false
            };
            
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            if (!ModelState.IsValid) return View("ChangeEmail", model);

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Action(
                "ConfirmEmail",
                "Account",
                new {user.Id, code},
                Request.Scheme);

            var message = new EmailMessage
            {
                Sender = new MailAddress {Email = _options.Email, Name = _options.Address},
                Subject = "Confermi la tua email?",
                Body = $"Ciao {user}, <br /> Per confermare la email <a href='{HtmlEncoder.Default.Encode(callbackUrl ?? string.Empty)}'>clicca qui!</a>.",
                To = new[] {new MailAddress {Name = user.UserName, Email = user.Email}}
            };

            await _email.SendAsync(message);

            ViewData["StatusMessage"] = "Email di verifica inviata. Controlla la tua casella di posta elettronica.";
            return View("ChangeEmail", model);
        }

        [HttpGet]
        public async Task<IActionResult> ChangeEmailConfirmation(string userId, string email, string code)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code))
                RedirectToAction("Index", "Home");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound($"Unable to load user with ID '{userId}'.");

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code ?? string.Empty));
            var result = await _userManager.ChangeEmailAsync(user, email, code);

            if (!result.Succeeded)
            {
                ViewData["StatusMessage"] = "Errore durante il cambio mail.";
                return View();
            }

            await _signInManager.RefreshSignInAsync(user);

            ViewData["StatusMessage"] = "Grazie per aver confermato il cambio email.";
            return View();
        }
    }
}