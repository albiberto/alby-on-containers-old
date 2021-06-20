using System.Linq;
using System.Threading.Tasks;
using IdentityServer.Areas.Roles.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Areas.Roles.Controllers
{
    [Authorize(Policy = "Admin"), Area("Roles")]
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
        public async Task<IActionResult> AddRoleAsync(string selectedRole)
        {
            if (!ModelState.IsValid) return View("Index", await BuildViewModelAsync());

            await _roleManager.CreateAsync(new() {Name = selectedRole});

            ViewData["StatusMessage"] = "Ruolo creato con successo";
            return View("Index", await BuildViewModelAsync());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoleAsync(RolesInputModel vm)
        {
            if (!ModelState.IsValid) return View("Index", await BuildViewModelAsync());

            var identityRole = await _roleManager.FindByNameAsync(vm.SelectedRole);
            await _roleManager.DeleteAsync(identityRole);
            
            ViewData["StatusMessage"] = "Ruolo eliminato con successo";
            return View("Index", await BuildViewModelAsync());
        }
        
        async Task<RolesViewModel> BuildViewModelAsync()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return new(roles.Select(role => role.Name));
        }
    }
}