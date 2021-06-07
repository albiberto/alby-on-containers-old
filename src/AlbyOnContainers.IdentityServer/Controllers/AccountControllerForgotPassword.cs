using System.Threading.Tasks;
using IdentityServer.ViewModels.AccountViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Controllers
{
    public partial class AccountController
    {
        [HttpGet, AllowAnonymous]
        public IActionResult ForgotPassword(string? returnUrl = default)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, string? returnUrl = default)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            
            // Don't reveal that the user does not exist or is not confirmed
            if (user == default || !await _userManager.IsEmailConfirmedAsync(user)) return View("ForgotPasswordConfirmation", model.Email);

            await PublishForgotPasswordEmailMessage(user, returnUrl);

            return View("ForgotPasswordConfirmation", model.Email);
        }

        
    }
}