using System;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using AlbyOnContainers.Messages;
using IdentityServer.Models;
using IdentityServer.Models.ManageViewModel;
using IdentityServer.Options;
using IdentityServer.Publishers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace IdentityServer.Controllers
{
    public class ManageController : Controller
    {
        readonly IEmailPublisher _email;
        readonly UserManager<ApplicationUser> _userManager;
        readonly EmailOptions _options;
        readonly SignInManager<ApplicationUser> _signInManager;

        public ManageController(IEmailPublisher email, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IOptions<EmailOptions> options)
        {
            _email = email;
            _userManager = userManager;
            _signInManager = signInManager;
            _options = options.Value;
        }

        #region PROFILE

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            var username = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var confirmed = await _userManager.IsPhoneNumberConfirmedAsync(user);

            var model = new ProfileViewModel { Username = username, PhoneNumber = phoneNumber ?? string.Empty, IsPhoneNumberConfirmed = confirmed };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            if (!ModelState.IsValid) return View(model);

            var username = await _userManager.GetUserNameAsync(user);
            if (!string.Equals(username, model.Username, StringComparison.InvariantCulture))
            {
                var setUsername = await _userManager.SetUserNameAsync(user, model.Username);
                if (!setUsername.Succeeded)
                {
                    ViewData["StatusMessage"] = "Ops... si &egrave; verificato un errore inaspettato durante il salvataggio dello username.";
                    return View(model);
                }
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (!string.Equals(model.PhoneNumber, phoneNumber, StringComparison.InvariantCulture))
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    ViewData["StatusMessage"] = "Ops... si &egrave; verificato un errore inaspettato durante il salvataggio del nuovo numero di telefono.";
                    return View(model);
                }
            }

            await _signInManager.RefreshSignInAsync(user);

            ViewData["StatusMessage"] = "Profilo aggiornato con successo";

            return View(model);
        }

        #endregion

        #region CHANGE EMAIL

        async Task<ChangeEmailViewModel> BuildViewModelAsync(ApplicationUser user)
        {
            var email = await _userManager.GetEmailAsync(user);
            var confirmed = await _userManager.IsEmailConfirmedAsync(user);

            return new()
            {
                NewEmail = email,
                Email = email,
                IsEmailConfirmed = confirmed
            };
        }

        [HttpGet]
        public async Task<IActionResult> ChangeEmail()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            var vm = await BuildViewModelAsync(user);
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            if (!ModelState.IsValid) return View(await BuildViewModelAsync(user));

            if (string.Equals(model.Email, model.NewEmail, StringComparison.InvariantCultureIgnoreCase))
            {
                ViewData["StatusMessage"] = "L'indirizzo inserito &egrave; uguale al precedente!";
                return View(await BuildViewModelAsync(user));
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var username = await _userManager.GetUserNameAsync(user);

            var code = await _userManager.GenerateChangeEmailTokenAsync(user, model.NewEmail);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Action(
                "ConfirmEmailChange",
                "Manage",
                new { userId, email = model.NewEmail, code },
                Request.Scheme);

            var message = new EmailMessage
            {
                Sender = new MailAddress { Email = _options.Email, Name = _options.Name },
                Subject = "Confermi la modifica della Email?",
                Body = $"Ciao {user}, <br /> Per confermare il cambio di email <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicca qui!</a>.",
                To = new[] { new MailAddress { Name = username, Email = model.NewEmail } }
            };

            await _email.SendAsync(message);

            ViewData["StatusMessage"] = "Ti abbiamo inviato una mail di conferma. Controlla la tua casella di posta elettronica.";
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SendVerificationEmailAsync(ChangeEmailViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            if (!ModelState.IsValid)
            {
                var vm = await BuildViewModelAsync(user);
                return View("ChangeEmail", vm);
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var username = await _userManager.GetUserNameAsync(user);
            var email = await _userManager.GetEmailAsync(user);

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Action(
                "ConfirmEmail",
                "Account",
                new { userId, code },
                Request.Scheme);

            var message = new EmailMessage
            {
                Sender = new MailAddress { Email = _options.Email, Name = _options.Name },
                Subject = "Confermi la tua email?",
                Body = $"Ciao {user}, <br /> Per confermare la email <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicca qui!</a>.",
                To = new[] { new MailAddress { Name = username, Email = email } }
            };

            await _email.SendAsync(message);

            ViewData["StatusMessage"] = "Email di verifica inviata. Controlla la tua casella di posta elettronica.";
            return View("ChangeEmail", model);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmailChange(string userId, string email, string code)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code)) RedirectToAction("Index", "Home");

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

        #endregion

        #region CHANGE PASSWORD

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            if (!ModelState.IsValid) return View(model);

            var checkPasswordResult = await _userManager.CheckPasswordAsync(user, model.OldPassword);
            if (!checkPasswordResult)
            {
                ModelState.AddModelError(nameof(model.OldPassword), "Password non valida");
                return View(model);
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(nameof(model.NewPassword), error.Description);
                }

                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);

            ViewData["StatusMessage"] = "Password modificata correttamente.";

            return View();
        }

        #endregion

        #region PERSONAL DATA

        [HttpGet]
        public async Task<IActionResult> PersonalData()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DownloadPersonalData()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            // Only include personal data for download
            var personalDataProps = typeof(IdentityUser).GetProperties()
                .Where(prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));

            var personalData = personalDataProps.ToDictionary(p => p.Name, p => p.GetValue(user)?.ToString() ?? "null");

            var logins = await _userManager.GetLoginsAsync(user);
            foreach (var login in logins)
            {
                personalData.Add($"{login.LoginProvider} external login provider key", login.ProviderKey);
            }

            Response.Headers.Add("Content-Disposition", "attachment; filename=PersonalData.json");

            return new FileContentResult(JsonSerializer.SerializeToUtf8Bytes(personalData), "application/json");
        }

        [HttpGet]
        public async Task<IActionResult> DeletePersonalData()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeletePersonalData(DeletePersonalDataViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            if (!ModelState.IsValid) return View();

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
            {
                ModelState.AddModelError(nameof(model.Password), "Password non valida");
                return View();
            }

            var result = await _userManager.DeleteAsync(user);

            var userId = await _userManager.GetUserIdAsync(user);
            if (!result.Succeeded) throw new InvalidOperationException($"Unexpected error occurred deleting user with ID '{userId}'.");

            await _signInManager.SignOutAsync();

            return Redirect("~/");
        }

        #endregion
    }
}