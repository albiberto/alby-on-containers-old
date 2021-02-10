using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Models.ManageViewModel
{
    public class ProfileViewModel
    {
        public string Username { get; set; }

        [Phone, DisplayName("Telefono")] public string PhoneNumber { get; set; }
        public bool IsPhoneNumberConfirmed { get; set; }

        public string StatusMessage { get; set; } = string.Empty;
    }
}