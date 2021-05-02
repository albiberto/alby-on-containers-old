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

        public RolesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index() => View(CreateModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRoles(UpdateUserRoleViewModel vm)
        {
            if (vm.Role == default)
            {
                ModelState.AddModelError(string.Empty, "You need to select a Role!");
                return View("Index", CreateModel());
            }

            if (vm.Delete)
            {
                var role = await _roleManager.FindByNameAsync(vm.Role);
                await _roleManager.DeleteAsync(role);
            }
            else
            {
                await _roleManager.CreateAsync(new IdentityRole { Name = vm.Role });
            }

            return View("Index", CreateModel());
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserRole(UpdateUserRoleViewModel vm)
        {
            if (vm.UserEmail == default)
            {
                ModelState.AddModelError(string.Empty, "You need to select an user !");
            }

            if (vm.Role == default)
            {
                ModelState.AddModelError(string.Empty, "You need to select a role to be added/deleted!");
            }

            if (!ModelState.IsValid)
            {
                return View("Index", CreateModel());
            }

            var user = await _userManager.FindByEmailAsync(vm.UserEmail);

            if (vm.Delete)
            {
                await _userManager.RemoveFromRoleAsync(user, vm.Role);
            }
            else
            {
                await _userManager.AddToRoleAsync(user, vm.Role);
            }

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