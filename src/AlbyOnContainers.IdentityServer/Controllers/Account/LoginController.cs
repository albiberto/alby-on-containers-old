using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer.Extensions;
using IdentityServer.Models;
using IdentityServer.ViewModels.Account;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Controllers.Account
{
    public partial class AccountController
    {
        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl = default)
        {
            // build a model so we know what to show on the login page
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            var vm = await BuildLoginViewModelAsync(context);

            // we only have one option for logging in and it's an external provider
            if (vm.IsExternalLoginOnly) return RedirectToAction("Challenge", "External", new {scheme = vm.ExternalLoginScheme, returnUrl});

            ViewData["ReturnUrl"] = returnUrl;
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model, string? returnUrl = default)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);

            if (!ModelState.IsValid) return View(await BuildLoginViewModelAsync(context, model));

            var user = await _userManager.FindByEmailAsync(model.Email);
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberLogin, true);

            if (!result.Succeeded)
            {
                await _events.RaiseAsync(new UserLoginFailureEvent(model.Email, "invalid credentials", clientId: context?.Client.ClientId));
                
                ModelState.AddModelError(string.Empty, _accountOptions.InvalidCredentialsErrorMessage);
                return View(await BuildLoginViewModelAsync(context, model));
            }

            await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName, clientId: context?.Client.ClientId));

            if (context != default)
            {
                return context.IsNativeClient()
                    // The client is native, so this change in how to return the response is for better UX for the end user.
                    ? this.LoadingPage("Redirect", returnUrl)
                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    : Redirect(returnUrl ?? "~/");
            }

            // request for a local page
            if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);

            if (string.IsNullOrEmpty(returnUrl)) return Redirect("~/");

            // user might have clicked on a malicious link - should be logged
            throw new Exception("invalid return URL");

        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(string? returnUrl = default)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);

            // since we don't have a valid context, then we just go back to the home page
            if (context == null) return Redirect("~/");

            // if the user cancels, send a result back into IdentityServer as if they 
            // denied the consent (even if this client does not require consent).
            // this will send back an access denied OIDC error response to the client.
            await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

            return context.IsNativeClient()
                // The client is native, so this change in how to return the response is for better UX for the end user.
                ? this.LoadingPage("Redirect", returnUrl)
                // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                : Redirect(returnUrl ?? "~/");
        }

        async Task<LoginViewModel> BuildLoginViewModelAsync(AuthorizationRequest? context, LoginInputModel? model = default)
        {
            if (!string.IsNullOrEmpty(context?.IdP) && await _schemeProvider.GetSchemeAsync(context.IdP) != default)
            {
                var local = context.IdP == IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                var externalProvider = !local ? new[] {new ExternalProvider {AuthenticationScheme = context.IdP}} : default;
                
                return new LoginViewModel(_accountOptions.AllowRememberLogin, local, externalProvider)
                {
                    Email = model?.Email ?? context.LoginHint,
                    RememberLogin = model?.RememberLogin ?? true
                };
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null)
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName ?? x.Name,
                    AuthenticationScheme = x.Name
                })
                .ToHashSet();

            var allowLocal = true;
            if (context?.Client.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.Client.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                        providers = providers.Where(provider =>
                            client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToHashSet();
                }
            }

            return new LoginViewModel(_accountOptions.AllowRememberLogin, allowLocal && _accountOptions.AllowLocalLogin, providers)
            {
                Email = model?.Email ?? context?.LoginHint,
                RememberLogin = model?.RememberLogin ?? true
            };
        }
    }
}