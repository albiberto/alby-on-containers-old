using System.Linq;
using System.Threading.Tasks;
using IdentityServer.Areas.Roles.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Areas.Roles.Controllers
{
    [Authorize(Roles = "Admin"), Area("Roles")]
    public class RolesController : Controller
    {
        readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            var vm = await BuildViewModelAsync();
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoleAsync(RolesInputModel vm)
        {
            if (!ModelState.IsValid) return View("Index", await BuildViewModelAsync());

            await _roleManager.CreateAsync(new() {Name = vm.SelectedRole});

            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoleAsync(RolesInputModel vm)
        {
            if (!ModelState.IsValid) return View("Index", await BuildViewModelAsync());

            var identityRole = await _roleManager.FindByNameAsync(vm.SelectedRole);
            await _roleManager.DeleteAsync(identityRole);

            return RedirectToAction("Index");
        }
        
        async Task<RolesViewModel> BuildViewModelAsync()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return new(roles.Select(role => role.Name));
        }
    }
}