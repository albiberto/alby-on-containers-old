using System.Threading.Tasks;
using IdentityServer.Areas.Manager.Models;
using IdentityServer.Models;
using IdentityServer.Options;
using IdentityServer.Publishers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace IdentityServer.Areas.Manager.Controllers
{
    [Authorize, Area("Manager")]
    public class ChangePasswordController : Controller
    {
        readonly SignInManager<ApplicationUser> _signInManager;
        readonly UserManager<ApplicationUser> _userManager;

        public ChangePasswordController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(ChangePasswordInputModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            var vm = model as ChangePasswordViewModel;
            
            if (!ModelState.IsValid) return View(vm);

            var checkPasswordResult = await _userManager.CheckPasswordAsync(user, model.OldPassword);
            if (!checkPasswordResult)
            {
                ModelState.AddModelError(nameof(model.OldPassword), "Password non valida");
                return View(vm);
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors) ModelState.AddModelError(string.Empty, error.Description);
                return View(vm);
            }

            await _signInManager.RefreshSignInAsync(user);

            ViewData["StatusMessage"] = "Password modificata correttamente.";

            return View();
        }
    }
}