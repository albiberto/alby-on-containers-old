using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer.Infrastructure
{
    public class ApplicationDbContextSeed
    {
        readonly IPasswordHasher<ApplicationUser> _passwordHasher = new PasswordHasher<ApplicationUser>();

        public async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();

            await AddRoles(scope);
            
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            
            var alby = userManager.FindByNameAsync("Albiberto").Result;
            if (alby == default)
            {
                alby = new ApplicationUser
                {
                    UserName = "Albiberto",
                    Email = "alberto.viezzi@albyoncontainers.world",
                    Id = $"{Guid.NewGuid()}",
                    PhoneNumber = "1234567890",
                    EmailConfirmed = true,
                    NormalizedEmail = "ALBERTO@ALBY.IT",
                    NormalizedUserName = "ALBIBERTO",
                    SecurityStamp = $"{Guid.NewGuid():D}"
                };
                
                var result = await userManager.CreateAsync(alby, "Pass123$");
                
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                var albyRole = await userManager.IsInRoleAsync(alby, "Admin");
                if (!albyRole) await userManager.AddToRoleAsync(alby, "Admin");
                    
                result = await userManager.AddClaimsAsync(alby, new Claim[]{
                    new (JwtClaimTypes.Name, "Alberto Viezzi"),
                    new (JwtClaimTypes.GivenName, "Alberto"),
                    new (JwtClaimTypes.FamilyName, "Viezzi")
                });

                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
            }
        }

        private async Task AddRoles(IServiceScope scope)
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            if (await roleManager.FindByNameAsync("User") == null) 
                await roleManager.CreateAsync(new IdentityRole() { Name = "User" });
            if (await roleManager.FindByNameAsync("Admin") == null) 
                await roleManager.CreateAsync(new IdentityRole() { Name = "Admin" });
        }
    }
}

// if (!context.Users.Any())
//             {
//                 var userMgr = services.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
//                 
//                 var users = GetDefaultUsers();
//                 var user1 = 
//                 await context.Users.AddRangeAsync(GetDefaultUsers());
//                 await context.SaveChangesAsync();
//             }
//
//             IEnumerable<ApplicationUser> GetDefaultUsers()
//             {
//                 var users = new[]
//                 {
//                     new ApplicationUser
//                     {
//                         Id = $"{Guid.NewGuid()}",
//                         PhoneNumber = "1234567890",
//                         Email = "alberto@alby.it",
//                         UserName = "alberto@alby.it",
//                         EmailConfirmed = true,
//                         NormalizedEmail = "ALBERTO@ALBY.IT",
//                         NormalizedUserName = "ALBERTO@ALBY.IT",
//                         SecurityStamp = $"{Guid.NewGuid():D}"
//                     },
//                     new ApplicationUser
//                     {
//                         Id = $"{Guid.NewGuid()}",
//                         PhoneNumber = "1234567890",
//                         Email = "yanier@alby.it",
//                         UserName = "yanier@alby.it",
//                         EmailConfirmed = true,
//                         NormalizedEmail = "YANIER@ALBY.IT",
//                         NormalizedUserName = "YANIER@ALBY.IT",
//                         SecurityStamp = $"{Guid.NewGuid():D}"
//                     },
//                     new ApplicationUser
//                     {
//                         Id = $"{Guid.NewGuid()}",
//                         PhoneNumber = "1234567890",
//                         Email = "roald@alby.it",
//                         UserName = "roald@alby.it",
//                         EmailConfirmed = true,
//                         NormalizedEmail = "ROALD@ALBY,IT",
//                         NormalizedUserName = "ROALD@ALBY,IT",
//                         SecurityStamp = $"{Guid.NewGuid():D}"
//                     }
//                 };
//
//                 foreach (var user in users) user.PasswordHash = _passwordHasher.HashPassword(user, "Pass@word1");
//
//                 return users;
//             }
//         }
//     }
// }