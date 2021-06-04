using System;
using System.Collections.Generic;
using System.Linq;
using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Infrastructure.Seeds.Config
{
    public static class ConfigIdentity
    {
        private const string Username1 = "Albiberto";
        private const string Username2 = "Crespoy";
        
        private static readonly IPasswordHasher<ApplicationUser> PasswordHasher = new PasswordHasher<ApplicationUser>();

        public static IReadOnlyCollection<ApplicationUser> Users(string password)
        {
            var users = new ApplicationUser[]
            {
                new()
                {
                    Name = "Alberto Viezzi",
                    GivenName = "Alberto",
                    FamilyName = "Viezzi",
                    UserName = Username1,
                    Email = "alberto.viezzi@albyoncontainers.world",
                    Id = $"{Guid.NewGuid()}",
                    PhoneNumber = "1234567890",
                    EmailConfirmed = true,
                    NormalizedEmail = "ALBERTO@ALBYONCONTAINERS.WORLD",
                    NormalizedUserName = "ALBIBERTO",
                    SecurityStamp = $"{Guid.NewGuid():D}"
                },
                new()
                {
                    Name = "Yanier Crespo",
                    GivenName = "Yanier",
                    FamilyName = "Crespo",
                    UserName = Username2,
                    Email = "yanier.crespo@albyoncontainers.world",
                    Id = $"{Guid.NewGuid()}",
                    PhoneNumber = "1234567890",
                    EmailConfirmed = true,
                    NormalizedEmail = "YANIER.CRESPO@ALBYONCONTAINERS.WORLD",
                    NormalizedUserName = "CRESPOY",
                    SecurityStamp = $"{Guid.NewGuid():D}"
                }
            };

            foreach (var user in users) user.PasswordHash = PasswordHasher.HashPassword(user, password);

            return users;
        }

        public static IReadOnlyCollection<string> GetRoles(string username) =>
            new Dictionary<string, IReadOnlyCollection<string>>
                {
                    {Username1, new List<string> {"Admin", "User"}},
                    {Username2, new List<string> {"User"}}
                }.SingleOrDefault(x => string.Equals(x.Key, username, StringComparison.InvariantCultureIgnoreCase))
                .Value;
    }
}