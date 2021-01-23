using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace IdentityServer.Infrastructure
{
    public class ApplicationDbContextSeed
    {
        readonly IPasswordHasher<ApplicationUser> _passwordHasher = new PasswordHasher<ApplicationUser>();

        public async Task SeedAsync(ApplicationDbContext context, ILogger<ApplicationDbContextSeed> logger, int retry = 10)
        {
            try
            {
                if (!context.Users.Any())
                {
                    await context.Users.AddRangeAsync(GetDefaultUsers());
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                if (retry < 10)
                {
                    retry++;

                    logger.LogError(ex, "EXCEPTION ERROR while migrating {DbContextName}", nameof(ApplicationDbContext));

                    await SeedAsync(context, logger, retry);
                }
            }
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