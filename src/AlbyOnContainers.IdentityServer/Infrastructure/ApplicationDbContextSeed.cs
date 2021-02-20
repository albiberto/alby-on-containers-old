using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Infrastructure
{
    public class ApplicationDbContextSeed
    {
        readonly IPasswordHasher<ApplicationUser> _passwordHasher = new PasswordHasher<ApplicationUser>();

        public async Task SeedAsync(ApplicationDbContext context)
        {
            if (!context.Users.Any())
            {
                await context.Users.AddRangeAsync(GetDefaultUsers());
                await context.SaveChangesAsync();
            }

            IEnumerable<ApplicationUser> GetDefaultUsers()
            {
                var users = new[]
                {
                    new ApplicationUser
                    {
                        Id = $"{Guid.NewGuid()}",
                        PhoneNumber = "1234567890",
                        Email = "alberto@alby.it",
                        UserName = "alberto@alby.it",
                        EmailConfirmed = true,
                        NormalizedEmail = "ALBERTO@ALBY.IT",
                        NormalizedUserName = "ALBERTO@ALBY.IT",
                        SecurityStamp = $"{Guid.NewGuid():D}"
                    },
                    new ApplicationUser
                    {
                        Id = $"{Guid.NewGuid()}",
                        PhoneNumber = "1234567890",
                        Email = "yanier@alby.it",
                        UserName = "yanier@alby.it",
                        EmailConfirmed = true,
                        NormalizedEmail = "YANIER@ALBY.IT",
                        NormalizedUserName = "YANIER@ALBY.IT",
                        SecurityStamp = $"{Guid.NewGuid():D}"
                    },
                    new ApplicationUser
                    {
                        Id = $"{Guid.NewGuid()}",
                        PhoneNumber = "1234567890",
                        Email = "roald@alby.it",
                        UserName = "roald@alby.it",
                        EmailConfirmed = true,
                        NormalizedEmail = "ROALD@ALBY,IT",
                        NormalizedUserName = "ROALD@ALBY,IT",
                        SecurityStamp = $"{Guid.NewGuid():D}"
                    }
                };

                foreach (var user in users) user.PasswordHash = _passwordHasher.HashPassword(user, "Pass@word1");

                return users;
            }
        }
    }
}