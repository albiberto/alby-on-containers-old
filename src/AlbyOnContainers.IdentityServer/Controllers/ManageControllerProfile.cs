using System;
using System.Threading.Tasks;
using IdentityServer.Models.ManageViewModel;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Controllers
{
    public partial class ManageController
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            var username = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var confirmed = await _userManager.IsPhoneNumberConfirmedAsync(user);

            var model = new ProfileViewModel
                {Username = username, PhoneNumber = phoneNumber ?? string.Empty, IsPhoneNumberConfirmed = confirmed};

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            if (!ModelState.IsValid) return View(model);

            var username = await _userManager.GetUserNameAsync(user);
            if (!string.Equals(username, model.Username, StringComparison.InvariantCulture))
            {
                var setUsername = await _userManager.SetUserNameAsync(user, model.Username);
                if (!setUsername.Succeeded)
                {
                    ViewData["StatusMessage"] =
                        "Ops... si &egrave; verificato un errore inaspettato durante il salvataggio dello username.";
                    return View(model);
                }
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (!string.Equals(model.PhoneNumber, phoneNumber, StringComparison.InvariantCulture))
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    ViewData["StatusMessage"] =
                        "Ops... si &egrave; verificato un errore inaspettato durante il salvataggio del nuovo numero di telefono.";
                    return View(model);
                }
            }

            await _signInManager.RefreshSignInAsync(user);

            ViewData["StatusMessage"] = "Profilo aggiornato con successo";

            return View(model);
        }
    }
}