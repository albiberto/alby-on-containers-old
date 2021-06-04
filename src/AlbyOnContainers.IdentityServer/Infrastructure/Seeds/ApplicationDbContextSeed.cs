using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer.Infrastructure.Seeds.Config;
using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer.Infrastructure.Seeds
{
    public static class ApplicationDbContextSeed
    {
        private const string Password = "Pass123$";
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var user in ConfigIdentity.Users(Password))
            {
                var stored = await userManager.FindByNameAsync(user.UserName);
                if (stored != default) continue;
                   
                var result = await userManager.CreateAsync(user, Password);
                
                if (!result.Succeeded) throw new Exception(result.Errors.First().Description);

                var roles = ConfigIdentity.GetRoles(user.UserName);
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                foreach (var role in roles)
                {
                    if (await roleManager.FindByNameAsync(role) == default) await roleManager.CreateAsync(new IdentityRole() { Name = role });
                    
                    var roleResult = await userManager.IsInRoleAsync(user, role);
                    if (!roleResult) await userManager.AddToRoleAsync(user, role);
                }
                
                result = await userManager.AddClaimsAsync(user, new Claim[]{
                    new (JwtClaimTypes.Name, user?.Name ?? string.Empty),
                    new (JwtClaimTypes.GivenName, user?.GivenName ?? string.Empty),
                    new (JwtClaimTypes.FamilyName, user?.FamilyName ?? string.Empty)
                });

                if (!result.Succeeded) throw new Exception(result.Errors.First().Description);
            }
        }
    }
}