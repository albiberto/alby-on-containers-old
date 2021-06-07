using System.Threading.Tasks;
using IdentityServer.Models;
using IdentityServer.ViewModels.AccountViewModels;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Controllers
{
    public  partial class AccountController
    {
        [HttpGet]
        public IActionResult Register(string? returnUrl = default)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = default)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser
            {
                Name = $"{model.GivenName} {model.FamilyName}", 
                GivenName = model.GivenName,
                FamilyName = model.FamilyName,
                UserName = model.Username ?? model.Email, 
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
                return View();
            }
            
            await PublishConfirmEmailMessage(user, returnUrl);
            
            return View("RegisterConfirmation", model.Email);
        }
    }
}