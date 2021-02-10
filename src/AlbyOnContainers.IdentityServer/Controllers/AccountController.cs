using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer.Exceptions;
using IdentityServer.Models;
using IdentityServer.Models.AccountViewModels;
using IdentityServer.Requests;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IdentityServer.Controllers
{
    public class AccountController : Controller
    {
        readonly IIdentityServerInteractionService _interaction;
        readonly ILogger<AccountController> _logger;
        readonly IMediator _mediator;

        public AccountController(IMediator mediator, IIdentityServerInteractionService interaction, ILogger<AccountController> logger)
        {
            _mediator = mediator;
            _interaction = interaction;
            _logger = logger;
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

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            ViewData["ReturnUrl"] = model.ReturnUrl;
            
            var host = $@"{Request.Scheme}://{Request.Host}/account/confirmemail";
            var request = new AccountRequests.Register(model.Username, model.Email, model.Password, host, model.ReturnUrl);
            
            if (ModelState.IsValid)
            {
                var result = await _mediator.Send(request);

                if (result.HasErrors())
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    // If we got this far, something failed, redisplay form
                    return View(model);
                }
            }

            if (request.ReturnUrl == null)
                return RedirectToAction("emailconfirmation", "account", new { model.Email });

            if (HttpContext?.User?.Identity?.IsAuthenticated ?? false)
                return Redirect(request.ReturnUrl);

            if (ModelState.IsValid)
                return RedirectToAction("login", "account", new { request.ReturnUrl });

            return View(model);
        }

        [HttpGet]
        public IActionResult EmailConfirmation([FromQuery] EmailConfirmationViewModel model) => View(model);

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail([FromQuery] ConfirmEmailViewModel model)
        {
            if (model.UserId == default || string.IsNullOrEmpty(model.Code)) return Redirect("/Index");

            try
            {
                var request = new AccountRequests.ConfirmEmail(model.UserId, model.Code);
                await _mediator.Publish(request);
            }
            catch (Exception e)
            {
                model.Message = "Si e' verificato un errore. Riprova piu' tardi.";
                return View(model.Message);
            }

            model.Message = "Grazie per aver confermato la tua email.";
            return View(model);
        }

        [HttpGet]
        public IActionResult ResendConfirmationEmail(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResendConfirmationEmail(ResendConfirmationEmailViewModel model)
        {
            if (!ModelState.IsValid) return View();

            try
            {
                var request = new AccountRequests.ResendConfirmationEmail(model.Email, $@"{Request.Scheme}://{Request.Host}/account/confirmemail", model.ReturnUrl);
                await _mediator.Publish(request);

                return RedirectToAction("emailconfirmation", "account", new { model.Email });
            }
            catch (Exception e)
            {
                if (e is not AuthenticationExceptions.EmailNotFound) return View("Error", new ErrorViewModel { Error = new ErrorMessage { Error = "Ops... si e' verificato un errore inaspettato!" } });

                ModelState.AddModelError(string.Empty, "L'email inserita non e' stata trovata!");
                return View();
            }
        }

        #endregion

        #region RecoverPassword

        [HttpGet, AllowAnonymous]
        public IActionResult RecoverPassword(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> RecoverPassword(RecoverPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var request = new AccountRequests.RecoverPassword(model.Email, $@"{Request.Scheme}://{Request.Host}/Account/ResetPassword", model.ReturnUrl);
                await _mediator.Publish(request);
            }
            catch (Exception e)
            {
                if (e is not AuthenticationExceptions) return View("Error", new ErrorViewModel { Error = new ErrorMessage { Error = "Ops... si e' verificato un errore inaspettato!" } });

                ModelState.AddModelError(string.Empty, "Email non ancora confermata!");
                return View();
            }

            return RedirectToAction("RecoverPasswordConfirmation", "account", new { model.Email });
        }

        [HttpGet]
        public IActionResult RecoverPasswordConfirmation([FromQuery] RecoverPasswordConfirmationViewModel model) => View(model);

        [HttpGet, AllowAnonymous]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            ViewData["ReturnUrl"] = model.ReturnUrl;

            if (string.IsNullOrEmpty(model.Code)) return RedirectToAction("Index", "Home");

            return View(model);
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPasswordPost(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View("ResetPassword", model);

            try
            {
                var request = new AccountRequests.ResetPassword(model.Email, model.Password, model.Code);
                await _mediator.Publish(request);
            }
            catch (Exception e)
            {
                if (e is not AuthenticationExceptions.UserNotFound) return View("Error", new ErrorViewModel { Error = new ErrorMessage { Error = "Ops... si e' verificato un errore inaspettato!" } });

                ModelState.AddModelError(string.Empty, "Email non ancora confermata!");
                return View("ResetPassword", model);
            }

            return RedirectToAction("ResetPasswordConfirmation", "Account");
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation() => View();

        #endregion
    }
}