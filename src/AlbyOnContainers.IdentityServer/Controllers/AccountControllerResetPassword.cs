using System.Text;
using System.Threading.Tasks;
using IdentityServer.ViewModels.AccountViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace IdentityServer.Controllers
{
    public partial class AccountController
    {
        [HttpGet]
        public IActionResult ResetPassword(string? code = default, string? returnUrl = default)
        {
            if (string.IsNullOrEmpty(code)) return BadRequest("A code must be supplied for password reset.");

            ViewData["ReturnUrl"] = returnUrl;
            return View(new ResetPasswordViewModel
            {
                Code = code
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, string? returnUrl = default)
        {
            if (!ModelState.IsValid) return View();

            // Don't reveal that the user does not exist
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return View("ResetPasswordConfirmation");

            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));
            var result = await _userManager.ResetPasswordAsync(user, code, model.Password);
            
            if (result.Succeeded) return View("ResetPasswordConfirmation");

            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            return View();
        }
    }
}