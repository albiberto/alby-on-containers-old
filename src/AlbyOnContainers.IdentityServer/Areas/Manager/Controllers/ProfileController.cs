using System;
using System.Security.Claims;
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
    public class ProfileController : Controller
    {
        readonly SignInManager<ApplicationUser> _signInManager;
        readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(IEmailPublisher email, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IOptions<EmailOptions> options)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            var model = new ProfileViewModel(user);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(ProfileInputModel model)
        {
            var vm = model as ProfileViewModel;
            
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            if (!ModelState.IsValid) return View(vm);

            var username = await _userManager.GetUserNameAsync(user);
            if (!string.Equals(username, model.Username, StringComparison.InvariantCulture))
            {
                var result = await _userManager.SetUserNameAsync(user, model.Username);
                if (!result.Succeeded)
                {
                    ViewData["StatusMessage"] = "Ops... si &egrave; verificato un errore inaspettato durante il salvataggio dello username.";
                    return View(vm);
                }
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (!string.Equals(model.PhoneNumber, phoneNumber, StringComparison.InvariantCulture))
            {
                var result = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!result.Succeeded)
                {
                    ViewData["StatusMessage"] = "Ops... si &egrave; verificato un errore inaspettato durante il salvataggio del nuovo numero di telefono.";
                    return View(vm);
                }
            }

            if (!string.Equals(model.GivenName, user.GivenName, StringComparison.OrdinalIgnoreCase) || !string.Equals(model.FamilyName, user.FamilyName, StringComparison.InvariantCultureIgnoreCase))
            {
                user.GivenName = model.GivenName;
                user.FamilyName = model.FamilyName;
                user.Name = $"{model.GivenName} {model.FamilyName}";

                var claims = await _userManager.GetClaimsAsync(user);

                foreach (var claim in claims)
                {
                    switch (claim.Type)
                    {
                        case "name":
                            await _userManager.ReplaceClaimAsync(user, claim, new Claim("name", user.Name));
                            break;
                        case "given_name":
                            await _userManager.ReplaceClaimAsync(user, claim, new Claim("given_name", user.GivenName ?? string.Empty));
                            break;
                        case "family_name":
                            await _userManager.ReplaceClaimAsync(user, claim, new Claim("family_name", user.FamilyName ?? string.Empty));
                            break;
                    }
                }
                
                
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    ViewData["StatusMessage"] = "Ops... si &egrave; verificato un errore inaspettato.";
                    return View(vm);
                }
            }
            
            await _signInManager.RefreshSignInAsync(user);

            ViewData["StatusMessage"] = "Profilo aggiornato con successo";

            return View(vm);
        }
    }
}