using System.Threading.Tasks;
using IdentityServer.ViewModels.ManageViewModel;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Controllers
{
    public partial class ManageController
    {
        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            if (!ModelState.IsValid) return View(model);

            var checkPasswordResult = await _userManager.CheckPasswordAsync(user, model.OldPassword);
            if (!checkPasswordResult)
            {
                ModelState.AddModelError(nameof(model.OldPassword), "Password non valida");
                return View(model);
            }

            var changePasswordResult =
                await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);

            ViewData["StatusMessage"] = "Password modificata correttamente.";

            return View();
        }
    }
}