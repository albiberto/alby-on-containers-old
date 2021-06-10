using System;
using System.Threading.Tasks;
using IdentityServer.Extensions;
using IdentityServer.ViewModels.AccountViewModels;
using IdentityServer4.Events;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Controllers
{
    public partial class AccountController
    {
        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl = default)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);

            var vm = new LoginViewModel
            {
                Email = context?.LoginHint ?? string.Empty
            };

            ViewData["ReturnUrl"] = returnUrl;
            return View(vm);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model, string? returnUrl = default)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);
                
                if (result.Succeeded)
                {
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName, clientId: context?.Client.ClientId));

                    if (context != null)
                    {
                        if (context.IsNativeClient())
                        {
                            // The client is native, so this change in how to
                            // return the response is for better UX for the end user.
                            return this.LoadingPage("Redirect", returnUrl);
                        }

                        // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                        return Redirect(returnUrl ?? "~/");
                    }

                    if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
                    if (string.IsNullOrEmpty(returnUrl)) return Redirect("~/");

                    // user might have clicked on a malicious link - should be logged
                    throw new Exception("invalid return URL");
                }

                await _events.RaiseAsync(new UserLoginFailureEvent(model.Email, "invalid credentials", clientId:context?.Client.ClientId));
                ModelState.AddModelError(string.Empty, "Invalid username or password");
            }

            // something went wrong, show form with error
            var vm = new LoginViewModel
            {
                Email = model.Email,
                Password = string.Empty,
                RememberMe = model.RememberMe
            };
            
            return View(vm);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(string? returnUrl = default)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);

            // since we don't have a valid context, then we just go back to the home page
            if (context == null) return Redirect("~/");
            
                // if the user cancels, send a result back into IdentityServer as if they 
                // denied the consent (even if this client does not require consent).
                // this will send back an access denied OIDC error response to the client.
                await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                if (context.IsNativeClient())
                {
                    // The client is native, so this change in how to
                    // return the response is for better UX for the end user.
                    return this.LoadingPage("Redirect", returnUrl);
                }

                return Redirect(returnUrl ?? "~/");

        }
    }
}