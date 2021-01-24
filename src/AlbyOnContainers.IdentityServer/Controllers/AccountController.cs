using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityModel;
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
using Microsoft.AspNetCore.WebUtilities;
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
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                }
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
                return await Logout(new AccountRequests.Logout {LogoutId = logoutId});

            //Test for Xamarin. 
            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
                //it's safe to automatically sign-out
                return await Logout(new AccountRequests.Logout {LogoutId = logoutId});

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

        [HttpGet, AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(AccountRequests.Register command)
        {
            ViewData["ReturnUrl"] = command.ReturnUrl;

            command.Host = $@"https://{Request.Host}/account/confirmemail";

            var model = new RegisterViewModel
            {
                Email = command.Email,
                Password = command.Password
            };

            if (ModelState.IsValid)
            {
                var result = await _mediator.Send(command);

                if (result.HasErrors())
                {
                    foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);

                    // If we got this far, something failed, redisplay form
                    return View(model);
                }
            }

            if (command.ReturnUrl == null)
                return RedirectToAction("registerconfirmation", "account");

            if (HttpContext?.User?.Identity?.IsAuthenticated ?? false)
                return Redirect(command.ReturnUrl);

            if (ModelState.IsValid)
                return RedirectToAction("login", "account", new {command.ReturnUrl});

            return View(model);
        }

        [HttpGet]
        public  IActionResult RegisterConfirmation(string email, string returnUrl = null)
        {
            return View();
        }

        [AllowAnonymous, HttpGet]
        public async Task<IActionResult> ConfirmEmail([FromQuery] AccountRequests.ConfirmEmail command)
        {
            if (command.UserId == default || command.Code == null)
            {
                return RedirectToPage("/Index");
            }

            var result = await _mediator.Send(command);

            var vm = new ConfirmEmailViewModel
            {
                Message = result.HasErrors()
                    ? "Error confirming your email."
                    : "Thank you for confirming your email."
            };

            return View(vm);
        }

        #endregion
    }
}