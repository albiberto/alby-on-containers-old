using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer.Infrastructure.Seeds.Config;
using IdentityServer.Models;
using Libraries.IHostExtensions;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Infrastructure.Seeds
{
    public class ApplicationDbContextSeed : IDbContextSeed<ApplicationDbContext>
    {
        readonly UserManager<ApplicationUser> _userManager;
        readonly RoleManager<IdentityRole> _roleManager;

        public ApplicationDbContextSeed(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        const string Password = "Pass123$";
        public async Task SeedAsync()
        {
            foreach (var user in ConfigIdentity.Users(Password))
            {
                var stored = await _userManager.FindByNameAsync(user.UserName);
                if (stored != default) continue;
                   
                var result = await _userManager.CreateAsync(user, Password);
                
                if (!result.Succeeded) throw new(result.Errors.First().Description);

                var roles = ConfigIdentity.GetRoles(user.UserName);

                foreach (var role in roles)
                {
                    if (await _roleManager.FindByNameAsync(role) == default) await _roleManager.CreateAsync(new() { Name = role });
                    
                    var roleResult = await _userManager.IsInRoleAsync(user, role);
                    if (!roleResult) await _userManager.AddToRoleAsync(user, role);
                }
                
                result = await _userManager.AddClaimsAsync(user, new Claim[]{
                    new (JwtClaimTypes.Name, user?.Name ?? string.Empty),
                    new (JwtClaimTypes.GivenName, user?.GivenName ?? string.Empty),
                    new (JwtClaimTypes.FamilyName, user?.FamilyName ?? string.Empty)
                });

                if (!result.Succeeded) throw new(result.Errors.First().Description);
            }
        }
    }
}