using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Models
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalDataAttribute]
        public string? Name { get; set; }
        [PersonalDataAttribute]
        public string? GivenName { get; set; }
        [PersonalDataAttribute]
        public string? FamilyName { get; set; }
    }
}