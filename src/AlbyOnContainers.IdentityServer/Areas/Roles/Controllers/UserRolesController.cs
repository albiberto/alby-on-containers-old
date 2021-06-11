using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer.Areas.Roles.Models;
using IdentityServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Areas.Roles.Controllers
{
    [Authorize(Roles = "Admin"), Area("Roles")]
    public class UserRolesController : Controller
    {
        readonly UserManager<ApplicationUser> _userManager;
        readonly RoleManager<IdentityRole> _roleManager;

        public UserRolesController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View("Index", new UserRolesViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([Required, EmailAddress] string? email = default)
        {
            if (!ModelState.IsValid) return View("Index", new UserRolesViewModel());

            var user = await _userManager.FindByEmailAsync(email);
            if (user == default)
            {
                ModelState.AddModelError(string.Empty, "User Not Found!");
                return View("Index", new UserRolesViewModel());
            }
            
            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.ToListAsync();
            
            var roles = allRoles.Select(role => new RoleViewModel(role.Name, userRoles.Contains(role.Name)));

            return View("Index", new UserRolesViewModel(user, roles));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(UserRolesInputModel model)
        {
            if (!ModelState.IsValid) return View("Index", new UserRolesViewModel());

            var user = await _userManager.FindByEmailAsync(model.Email);

            foreach (var role in model.SelectedRoles)
            {
                if (!await _userManager.IsInRoleAsync(user, role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }
            
            foreach (var role in model.AllRoles.Except(model.SelectedRoles))
            {
                if (await _userManager.IsInRoleAsync(user, role))
                {
                    await _userManager.RemoveFromRoleAsync(user, role);
                }
            }
            
            return RedirectToAction("Index", new UserRolesViewModel(user));
        }
    }
}