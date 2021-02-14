using System;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AlbyOnContainers.Messages;
using IdentityModel;
using IdentityServer.Models;
using IdentityServer.Models.AccountViewModels;
using IdentityServer.Options;
using IdentityServer.Publishers;
using IdentityServer.Requests;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IdentityServer.Controllers
{
    public class AccountController : Controller
    {
        readonly IIdentityServerInteractionService _interaction;
        readonly EmailOptions _options;
        readonly ILogger<AccountController> _logger;
        readonly IMediator _mediator;
        readonly UserManager<ApplicationUser> _userManager;
        readonly SignInManager<ApplicationUser> _signInManager;
        readonly IEmailPublisher _publisher;

        public AccountController(IMediator mediator, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailPublisher publisher, IIdentityServerInteractionService interaction, IOptions<EmailOptions> options, ILogger<AccountController> logger)
        {
            _mediator = mediator;
            _userManager = userManager;
            _signInManager = signInManager;
            _publisher = publisher;
            _interaction = interaction;
            _options = options.Value;
            _logger = logger;
        }

        async Task<(string userId, string username, string email)> GetUserInfoAsync(ApplicationUser user)
        {
            var userId = await _userManager.GetUserIdAsync(user);
            var username = await _userManager.GetUserNameAsync(user);
            var email = await _userManager.GetEmailAsync(user);

            return (userId, username, email);
        }

        #region Login

        /// <summary>
        ///     Show login page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null) throw new NotImplementedException("External login is not implemented!");

            var vm = BuildLoginViewModel(returnUrl, context);

            ViewData["ReturnUrl"] = returnUrl;

            return View(vm);
        }

        /// <summary>
        ///     Handle postback from username/password login
        /// </summary>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AccountRequests.Login request)
        {
            if (ModelState.IsValid)
            {
                var result = await _mediator.Send(request);

                if (result.HasErrors())
                    ModelState.AddModelError("", "Credenziali invalide.");
                else
                {
                    // make sure the returnUrl is still valid, and if yes - redirect back to authorize endpoint
                    var url = _interaction.IsValidReturnUrl(request.ReturnUrl) ? request.ReturnUrl : "~/";
                    return Redirect(url);
                }
            }

            // something went wrong, show form with error
            var vm = await BuildLoginViewModelAsync(request.ReturnUrl, request.Email, request.RememberMe);
            ViewData["ReturnUrl"] = request.ReturnUrl;

            return View(vm);
        }

        async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl, string email, bool rememberMe)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            var vm = BuildLoginViewModel(returnUrl, context);
            vm.Email = email;
            vm.RememberMe = rememberMe;
            return vm;
        }

        static LoginViewModel BuildLoginViewModel(string returnUrl, AuthorizationRequest context) =>
            new()
            {
                ReturnUrl = returnUrl,
                Email = context?.LoginHint
            };

        #endregion

        #region LogOut

        /// <summary>
        ///     Show logout page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            if (User?.Identity?.IsAuthenticated ?? false)
                // if the user is not authenticated, then just show logged out page
                return await Logout(new AccountRequests.Logout { LogoutId = logoutId });

            //Test for Xamarin. 
            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
                //it's safe to automatically sign-out
                return await Logout(new AccountRequests.Logout { LogoutId = logoutId });

            // show the logout prompt. this prevents attacks where the user is automatically signed out by another malicious web page.
            var vm = new LogoutViewModel
            {
                LogoutId = logoutId
            };

            return View(vm);
        }

        /// <summary>
        ///     Handle logout page postback
        /// </summary>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(AccountRequests.Logout model)
        {
            var idp = User?.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;

            if (idp != null && idp != IdentityServerConstants.LocalIdentityProvider)
            {
                model.LogoutId ??= await _interaction.CreateLogoutContextAsync();

                var url = "/Account/Logout?logoutId=" + model.LogoutId;

                try
                {
                    // hack: try/catch to handle social providers that throw
                    await HttpContext.SignOutAsync(idp, new AuthenticationProperties
                    {
                        RedirectUri = url
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "LOGOUT ERROR: {ExceptionMessage}", ex.Message);
                }
            }

            // delete authentication cookie
            await HttpContext.SignOutAsync();
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            // set this so UI rendering sees an anonymous user
            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(model.LogoutId);

            return Redirect(logout?.PostLogoutRedirectUri ?? "/");
        }

        public async Task<IActionResult> DeviceLogOut(string redirectUrl)
        {
            // delete authentication cookie
            await HttpContext.SignOutAsync();

            // set this so UI rendering sees an anonymous user
            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            return Redirect(redirectUrl);
        }

        #endregion

        #region Register

        [HttpGet]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = default)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser { UserName = model.Username ?? model.Email, Email = model.Email };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View();
            }

            var (userId, username, email) = await GetUserInfoAsync(user);

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Action(
                "ConfirmEmail",
                "Account",
                new { userId, code, returnUrl },
                Request.Scheme);

            var message = new EmailMessage
            {
                Sender = new MailAddress { Email = _options.Email, Name = _options.Name },
                Subject = "Conferma Email",
                Body = $"Ciao {username}, <br /> Per confermare il tuo account <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicca qui!</a>.",
                To = new[] { new MailAddress { Name = username, Email = email } }
            };

            await _publisher.SendAsync(message);

            return View("RegisterConfirmation", email);
        }

        [HttpGet]
        public IActionResult ResendEmail(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResendEmail(ResendConfirmationEmailViewModel model, string returnUrl = default)
        {
            if (!ModelState.IsValid) return View();

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return View("ResendEmailConfirmation", model.Email);

            var (userId, username, email) = await GetUserInfoAsync(user);

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Action(
                "ConfirmEmail",
                "Account",
                new { userId, code, returnUrl },
                Request.Scheme);

            var message = new EmailMessage
            {
                Sender = new MailAddress { Email = _options.Email, Name = _options.Name },
                Subject = "Conferma Email",
                Body = $"Ciao {user}, <br /> Per confermare il tuo account <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicca qui!</a>.",
                To = new[] { new MailAddress { Name = username, Email = email } }
            };

            await _publisher.SendAsync(message);

            return View("ResendEmailConfirmation", email);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code, string returnUrl = default)
        {
            ViewData["ReturnRul"] = returnUrl;

            if (userId == null || code == null) return RedirectToPage("/Index");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound($"Unable to load user with ID '{userId}'.");

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            var result = await _userManager.ConfirmEmailAsync(user, code);

            ViewData["StatusMessage"] = result.Succeeded ? "Grazie per aver confermato la tua email." : "Ops... si e' verificato un errore durante la consegna della mail.";
            return View();
        }

        #endregion

        #region ForgotPassword

        [HttpGet, AllowAnonymous]
        public IActionResult ForgotPassword(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(RecoverPasswordViewModel model, string returnUrl = default)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
                // Don't reveal that the user does not exist or is not confirmed
                return View("ForgotPasswordConfirmation", model.Email);

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var (_, username, email) = await GetUserInfoAsync(user);

            var callbackUrl = Url.Action(
                "ResetPassword",
                "Account",
                new { code, returnUrl },
                Request.Scheme);

            var message = new EmailMessage
            {
                Sender = new MailAddress { Email = _options.Email, Name = _options.Name },
                Subject = "Recupero password",
                Body = $"Ciao {username}, <br /> Per recuperare la tua password <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicca qui!</a>.",
                To = new[] { new MailAddress { Name = username, Email = email } }
            };

            await _publisher.SendAsync(message);

            return View("ForgotPasswordConfirmation", email);
        }

        [HttpGet]
        public IActionResult ResetPassword(string code = default, string returnUrl = default)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrEmpty(code)) return BadRequest("A code must be supplied for password reset.");

            var model = new ResetPasswordViewModel { Code = code };
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, string returnUrl = default)
        {
            if (!ModelState.IsValid) return View();

            var user = await _userManager.FindByEmailAsync(model.Email);

            // Don't reveal that the user does not exist
            if (user == null) return View("ResetPasswordConfirmation");

            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));
            var result = await _userManager.ResetPasswordAsync(user, code, model.Password);
            if (result.Succeeded) return View("ResetPasswordConfirmation");

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View();
        }

        #endregion
    }
}