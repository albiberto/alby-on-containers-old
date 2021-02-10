using System;
using System.Threading.Tasks;
using IdentityServer.Models;
using IdentityServer.Models.ManageViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Controllers
{
    public class ManageController : Controller
    {
        readonly UserManager<ApplicationUser> _userManager;
        readonly SignInManager<ApplicationUser> _signInManager;

        public ManageController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            var username = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var confirmed = await _userManager.IsPhoneNumberConfirmedAsync(user);

            var model = new ProfileViewModel { Username = username, PhoneNumber = phoneNumber ?? string.Empty, IsPhoneNumberConfirmed = confirmed };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            var username = await _userManager.GetUserNameAsync(user);
            if (!string.Equals(username, model.Username, StringComparison.InvariantCulture))
            {
                var setUsername = await _userManager.SetUserNameAsync(user, model.Username);
                if (!setUsername.Succeeded)
                {
                    model.StatusMessage = "Unexpected error when trying to set phone number.";
                    return View(model);
                }
            }
            
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (!string.Equals(model.PhoneNumber, phoneNumber, StringComparison.InvariantCulture))
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    model.StatusMessage = "Unexpected error when trying to set phone number.";
                    return View(model);
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            
            model.StatusMessage = "Your profile has been updated";

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ChangeEmail()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            var email = await _userManager.GetEmailAsync(user);
            var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

            var model = new ChangeEmailViewModel
            {
                Email = email,
                NewEmail = email,
                IsEmailConfirmed = isEmailConfirmed
            };

            return View(model);
        }
        
        [HttpGet]
        public IActionResult ChangePassword() => View();

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);

            model.StatusMessage = "Your password has been changed.";

            return View(model);
        }
    }
}