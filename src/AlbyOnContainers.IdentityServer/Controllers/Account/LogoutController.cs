using System;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer.ViewModels.Account;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Controllers.Account
{
    public partial class AccountController
    {
        /// <summary>
        /// Show logout page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout(string? logoutId)
        {
            // build a model so the logout page knows what to display
            var vm = await BuildLogoutViewModelAsync(logoutId);

            if (!vm.ShowLogoutPrompt)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await Logout(vm);
            }

            return View(vm);
        }

        /// <summary>
        /// Handle logout page postback
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // build a model so the logged out page knows what to display
            var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

            if (User.Identity?.IsAuthenticated ?? false)
            {
                // delete local authentication cookie
                await _signInManager.SignOutAsync();

                // raise the logout event
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            // check if we need to trigger sign-out at an upstream identity provider
            if (!vm.TriggerExternalSignOut) return View("LoggedOut", vm);
            
            // build a return URL so the upstream provider will redirect back
            // to us after the user has logged out. this allows us to then
            // complete our single sign-out processing.
            var url = Url.Action("Logout", new { logoutId = vm.LogoutId });

            // this triggers a redirect to the external provider for sign-out
            return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme ?? string.Empty);
        }

        async Task<LogoutViewModel> BuildLogoutViewModelAsync(string? logoutId = default)
        {
            var context = await _interaction.GetLogoutContextAsync(logoutId);

            var show = !(User.Identity?.IsAuthenticated != true || context?.ShowSignoutPrompt == false) && _accountOptions.ShowLogoutPrompt;
            
            return new LogoutViewModel(show) { LogoutId = logoutId };
        }

        async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string? logoutId = default)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(logoutId);
            string? schema = default;

            if (User.Identity?.IsAuthenticated == true)
            {
                var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (!string.IsNullOrEmpty(idp) && idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignOut = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignOut)
                    {
                        if (string.IsNullOrEmpty(logoutId))
                        {
                            // if there's no current logout context, we need to create one
                            // this captures necessary info from the current logged in user
                            // before we signout and redirect away to the external IdP for signout
                            logoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        schema = idp;
                    }
                }
            }

            return new LoggedOutViewModel(logout?.ClientName ?? logout?.ClientId, logoutId, logout?.PostLogoutRedirectUri ?? "/", logout?.SignOutIFrameUrl, _accountOptions.AutomaticRedirectAfterSignOut, schema);
        }
    }
}