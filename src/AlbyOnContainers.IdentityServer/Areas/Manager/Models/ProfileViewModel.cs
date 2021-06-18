using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using IdentityServer.Models;

namespace IdentityServer.Areas.Manager.Models
{
    public record ProfileViewModel : ProfileInputModel
    {
        public ProfileViewModel(ApplicationUser user) : this(user.UserName, user.FamilyName, user.GivenName, user.PhoneNumber, user.PhoneNumberConfirmed)
        {
        }
        public ProfileViewModel(string? username, string? familyName, string? givenName, string? phoneNumber, bool phoneNumberConfirmed)
        {
            Username = username;
            FamilyName = familyName;
            GivenName = givenName;
            PhoneNumber = phoneNumber;
            PhoneNumberConfirmed = phoneNumberConfirmed;
        }
    }
}