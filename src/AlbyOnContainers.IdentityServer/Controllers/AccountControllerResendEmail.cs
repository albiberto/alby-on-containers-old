using System.Threading.Tasks;
using IdentityServer.ViewModels.AccountViewModels;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Controllers
{
    public partial class AccountController
    {
        [HttpGet]
        public IActionResult ResendEmail(string? returnUrl = default)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResendEmail(ResendConfirmationEmailViewModel model, string? returnUrl = default)
        {
            if (!ModelState.IsValid) return View();

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != default) await PublishConfirmEmailMessage(user, returnUrl);
            
            return View("ResendEmailConfirmation", model.Email);
        }
    }
}