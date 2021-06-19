using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using IdentityServer.Areas.Manager.Models;
using IdentityServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Areas.Manager.Controllers
{
    [Authorize(Policy = "All"), Area("Manager")]
    public class PersonalDataController : Controller
    {
        readonly SignInManager<ApplicationUser> _signInManager;
        readonly UserManager<ApplicationUser> _userManager;

        public PersonalDataController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DownloadPersonalData()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            // Only include personal data for download
            var personalDataProps = typeof(ApplicationUser).GetProperties()
                .Where(prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));

            var personalData = personalDataProps.ToDictionary(p => p.Name, p => p.GetValue(user)?.ToString() ?? "null");

            var logins = await _userManager.GetLoginsAsync(user);
            foreach (var login in logins)
                personalData.Add($"{login.LoginProvider} external login provider key", login.ProviderKey);

            Response.Headers.Add("Content-Disposition", "attachment; filename=PersonalData.json");

            return new FileContentResult(JsonSerializer.SerializeToUtf8Bytes(personalData), "application/json");
        }

        [HttpGet]
        public async Task<IActionResult> PersonalDataDelete()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            return View("PersonalDataDelete");
        }

        [HttpPost]
        public async Task<IActionResult> PersonalDataDelete(DeletePersonalDataInputModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            if (!ModelState.IsValid) return View();

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
            {
                ModelState.AddModelError(nameof(model.Password), "Password non valida");
                return View("PersonalDataDelete");
            }

            var result = await _userManager.DeleteAsync(user);

            var userId = await _userManager.GetUserIdAsync(user);
            if (!result.Succeeded) throw new InvalidOperationException($"Unexpected error occurred deleting user with ID '{userId}'.");

            await _signInManager.SignOutAsync();

            return Redirect("~/");
        }
    }
}