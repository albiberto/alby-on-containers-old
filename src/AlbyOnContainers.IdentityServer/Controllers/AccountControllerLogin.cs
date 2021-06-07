using System;
using System.Threading.Tasks;
using IdentityServer.Extensions;
using IdentityServer.ViewModels.AccountViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Controllers
{
    public partial class AccountController
    {
        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl = default)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            
            if (context?.IdP != null) throw new NotImplementedException("External login is not implemented!");

            var vm = new LoginViewModel
            {
                Email = context?.LoginHint ?? string.Empty
            };
            ViewData["ReturnUrl"] = returnUrl;
            
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
        {  
            if (!ModelState.IsValid) return View(model);
            
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (!await _userManager.CheckPasswordAsync(user, model.Password))
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }
            
            var props = new AuthenticationProperties
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(_tokenOptions.Minutes),
                AllowRefresh = true,
                RedirectUri = returnUrl
            };
            
            if (model.RememberMe)
            {
                props.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(_tokenOptions.Days);
                props.IsPersistent = true;
            }
            
            await _signInManager.SignInAsync(user, props);
            
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context != default && (await _clientStore.IsPkceClientAsync(context.Client.ClientId) || context.IsNativeClient()))
            {
                return View("Redirect", new RedirectViewModel { RedirectUrl = returnUrl ?? "/"});
            }
            
            return Redirect(returnUrl ?? "~/");
        }
    }
}