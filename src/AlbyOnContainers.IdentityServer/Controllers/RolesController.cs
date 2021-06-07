using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer.Infrastructure;
using IdentityServer.Models;
using IdentityServer.Models.RolesViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index() => View(CreateModel());

        [HttpPost]
        public async Task<IActionResult> AddUserInRole(UpdateUserRoleViewModel vm) => 
            await UpsertUserRole(vm, (user, role) => _userManager.AddToRoleAsync(user, role));
  
        [HttpPost]
        public async Task<IActionResult> RemoveUserFromRole(UpdateUserRoleViewModel vm) => 
            await UpsertUserRole(vm, (user, role) => _userManager.RemoveFromRoleAsync(user, role));

        private async Task<IActionResult> UpsertUserRole(UpdateUserRoleViewModel vm, Func<ApplicationUser, string, Task> selector)
        {
            if (string.IsNullOrEmpty(vm.UserEmail)) ModelState.AddModelError(string.Empty, "You need to select an user !");
            if (string.IsNullOrEmpty(vm.CurrentRole)) ModelState.AddModelError(string.Empty, "You need to select a role to be added/deleted!");

            if (!ModelState.IsValid) return View("Index", CreateModel());

            var user = await _userManager.FindByEmailAsync(vm.UserEmail);
            
            await selector(user, vm.CurrentRole!);

            return View("Index", CreateModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRole(UpdateUserRoleViewModel vm) => await UpsertRole(vm.UpsertRole, role => 
            _roleManager.CreateAsync(new IdentityRole {Name = role}));
    
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(UpdateUserRoleViewModel vm) => await UpsertRole(vm.UpsertRole, async role =>
        {
            var r = await _roleManager.FindByNameAsync(role);
            await _roleManager.DeleteAsync(r);
        });

        private async Task<IActionResult> UpsertRole(string? role, Func<string, Task> selector)
        {
            if (string.IsNullOrEmpty(role))
            {
                ModelState.AddModelError(string.Empty, "You need to select a Role!");
                return View("Index", CreateModel());
            }

            await selector(role);

            return View("Index", CreateModel());
        }
        
        private UpdateUserRoleViewModel CreateModel()
        {
            var convertedUsers = _userManager.Users.ToArray()
                .Select(x => new UsersViewModel
                {
                    Email = x.Email,
                    Roles = _roleManager.Roles.ToArray()
                        .Where(y => _context.UserRoles.ToArray().Any(z => z.UserId == x.Id && z.RoleId == y.Id))
                        .Select(y => new UsersRole
                        {
                            Name = y.NormalizedName
                        })
                });
            return new UpdateUserRoleViewModel
            {
                Roles = _roleManager.Roles.Select(x => x.NormalizedName),
                Users = convertedUsers
            };
        }
    }
}