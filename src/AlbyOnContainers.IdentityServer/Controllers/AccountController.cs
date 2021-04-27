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
    public class AccountController : Controller
    {
        readonly EmailOptions _emailOptions;
        readonly IIdentityServerInteractionService _interaction;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        readonly ILogger<AccountController> _logger;
        readonly IEmailPublisher _publisher;
        readonly SignInManager<ApplicationUser> _signInManager;
        readonly TokenLifetimeOptions _tokenOptions;
        readonly UserManager<ApplicationUser> _userManager;
        private readonly IClientStore _clientStore;

        public AccountController(
            UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            IEmailPublisher publisher, IIdentityServerInteractionService interaction,
            IOptions<EmailOptions> emailOptions, IOptions<TokenLifetimeOptions> tokenOptions,
            ILogger<AccountController> logger, IClientStore clientStore, IAuthenticationSchemeProvider schemeProvider)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _publisher = publisher;
            _interaction = interaction;
            _emailOptions = emailOptions.Value;
            _tokenOptions = tokenOptions.Value;
            _logger = logger;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
        }

        async Task<(string userId, string username, string email)> GetUserInfoAsync(ApplicationUser user)
        {
            var userId = await _userManager.GetUserIdAsync(user);
            var username = await _userManager.GetUserNameAsync(user);
            var email = await _userManager.GetEmailAsync(user);

            return (userId, username, email);
        }

        #region Login

        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            // build a model so we know what to show on the login page
            var vm = await BuildLoginViewModelAsync(returnUrl);

            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return View(); //RedirectToAction("Challenge", "External", new { provider = vm.ExternalLoginScheme, returnUrl });
            }

            return View(vm);
            // ViewData["ReturnUrl"] = returnUrl;
            //            
            // var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            // if (context?.IdP != null) throw new NotImplementedException("External login is not implemented!");
            //
            // var model = new LoginInputModel {Email = context?.LoginHint};
            // return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string button)
        {  
            if (!ModelState.IsValid) return View(model);
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
            
            var user = await _userManager.FindByEmailAsync(model.Email);
            
            if (await _userManager.CheckPasswordAsync(user, model.Password))
            { 
                var props = new AuthenticationProperties
                {
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(_tokenOptions.Minutes),
                    AllowRefresh = true,
                    RedirectUri = model.ReturnUrl
                };
            
                if (model.RememberMe)
                {
                    props.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(_tokenOptions.Days);
                    props.IsPersistent = true;
                }
            
                await _signInManager.SignInAsync(user, props);
                if (context != null)
                {
                    if (await _clientStore.IsPkceClientAsync(context.Client.ClientId))
                    {
                        // if the client is PKCE then we assume it's native, so this change in how to
                        // return the response is for better UX for the end user.
                        return this.LoadingPage("Redirect", model.ReturnUrl);
                    }
                    
                    if (context.IsNativeClient())
                    {
                        // The client is native, so this change in how to
                        // return the response is for better UX for the end user.
                        return this.LoadingPage("Redirect", model.ReturnUrl);
                    }

                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    return Redirect(model.ReturnUrl);
                }

                // request for a local page
                if (Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }
                else if (string.IsNullOrEmpty(model.ReturnUrl))
                {
                    return Redirect("~/");
                }
                else
                {
                    // user might have clicked on a malicious link - should be logged
                    throw new Exception("invalid return URL");
                }
                // make sure the returnUrl is still valid, and if yes - redirect back to authorize endpoint
                return Redirect(_interaction.IsValidReturnUrl(model.ReturnUrl) ? props.RedirectUri : "~/");
            }
            
            // something went wrong, show form with error
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        #endregion

        #region LogOut

        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId = default)
        {
            var model = new LogoutViewModel {LogoutId = logoutId};

            // if the user is not authenticated, then just show logged out page
            if (!(User?.Identity?.IsAuthenticated ?? true)) return await Logout(model);

            // show the logout prompt. this prevents attacks where the user is automatically signed out by another malicious web page.
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutViewModel model)
        {
            var idp = User?.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;

            if (idp != null && idp != IdentityServerConstants.LocalIdentityProvider)
            {
                // if there's no current logout context, we need to create one
                // this captures necessary info from the current logged in user
                // before we signout and redirect away to the external IdP for signout
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

            return Redirect(logout?.PostLogoutRedirectUri ?? "~/");
        }

        public async Task<IActionResult> DeviceLogOut(string returnUrl)
        {
            // delete authentication cookies
            await HttpContext.SignOutAsync();

            // set this so UI rendering sees an anonymous user
            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            return Redirect(returnUrl);
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

            var user = new ApplicationUser {UserName = model.Username ?? model.Email, Email = model.Email};

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);

                return View();
            }

            var (userId, username, email) = await GetUserInfoAsync(user);

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Action(
                "ConfirmEmail",
                "Account",
                new {userId, code, returnUrl},
                Request.Scheme);

            var message = new EmailMessage
            {
                Sender = new MailAddress {Email = _emailOptions.Email, Name = _emailOptions.Address},
                Subject = "Conferma Email",
                Body =
                    $"Ciao {username}, <br /> Per confermare il tuo account <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicca qui!</a>.",
                To = new[] {new MailAddress {Name = username, Email = email}}
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
                new {userId, code, returnUrl},
                Request.Scheme);

            var message = new EmailMessage
            {
                Sender = new MailAddress {Email = _emailOptions.Email, Name = _emailOptions.Address},
                Subject = "Conferma Email",
                Body =
                    $"Ciao {user}, <br /> Per confermare il tuo account <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicca qui!</a>.",
                To = new[] {new MailAddress {Name = username, Email = email}}
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

            ViewData["StatusMessage"] = result.Succeeded
                ? "Grazie per aver confermato la tua email."
                : "Ops... si e' verificato un errore durante la consegna della mail.";
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

            // Don't reveal that the user does not exist or is not confirmed
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == default || !await _userManager.IsEmailConfirmedAsync(user))
                return View("ForgotPasswordConfirmation", model.Email);

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var (_, username, email) = await GetUserInfoAsync(user);

            var callbackUrl = Url.Action(
                "ResetPassword",
                "Account",
                new {code, returnUrl},
                Request.Scheme);

            var message = new EmailMessage
            {
                Sender = new MailAddress {Email = _emailOptions.Email, Name = _emailOptions.Address},
                Subject = "Recupero password",
                Body =
                    $"Ciao {username}, <br /> Per recuperare la tua password <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicca qui!</a>.",
                To = new[] {new MailAddress {Name = username, Email = email}}
            };

            await _publisher.SendAsync(message);

            return View("ForgotPasswordConfirmation", email);
        }

        [HttpGet]
        public IActionResult ResetPassword(string code = default, string returnUrl = default)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrEmpty(code)) return BadRequest("A code must be supplied for password reset.");

            var model = new ResetPasswordViewModel {Code = code};
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

            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);

            return View();
        }

        #endregion
         private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
            {
                var local = context.IdP == IdentityServer4.IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                var vm = new LoginViewModel
                {
                    EnableLocalLogin = local,
                    ReturnUrl = returnUrl,
                    Email = context?.LoginHint,
                };

                if (!local)
                {
                    vm.ExternalProviders = new[] { new ExternalProvider { AuthenticationScheme = context.IdP } };
                }

                return vm;
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null ||
                            (x.Name.Equals(AccountOptions.WindowsAuthenticationSchemeName, StringComparison.OrdinalIgnoreCase))
                )
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName ?? x.Name,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            if (context?.Client.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.Client.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            return new LoginViewModel
            {
                AllowRememberLogin = AccountOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Email = context?.LoginHint,
                ExternalProviders = providers.ToArray()
            };
        }
    }
}