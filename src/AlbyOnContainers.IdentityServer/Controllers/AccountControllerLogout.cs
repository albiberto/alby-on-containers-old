using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer.ViewModels.AccountViewModels;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IdentityServer.Controllers
{
    public partial class AccountController
    {
                #region LogOut

        [HttpGet]
        public async Task<IActionResult> Logout(string? logoutId = default)
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
                // if there's no current logout context, we need to create one this captures necessary info from the current logged in userbefore we signout and redirect away to the external IdP for signout
                model.LogoutId ??= await _interaction.CreateLogoutContextAsync();

                try
                {
                    // try/catch to handle social providers that throw
                    await HttpContext.SignOutAsync(idp, new AuthenticationProperties
                    {
                        RedirectUri = $"/Account/Logout?logoutId={model.LogoutId}"
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

        #endregion
    }
}