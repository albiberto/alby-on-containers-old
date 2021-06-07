using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer.ViewModels.ManageViewModel
{
    public class ProfileViewModel
    {
        [MinLength(3)] public string Username { get; set; }

        [Phone, DisplayName("Telefono")] public string PhoneNumber { get; set; }
        public bool IsPhoneNumberConfirmed { get; set; }
    }
}