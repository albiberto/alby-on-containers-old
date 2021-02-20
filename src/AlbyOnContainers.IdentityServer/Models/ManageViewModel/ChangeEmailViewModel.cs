using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Models.ManageViewModel
{
    public class ChangeEmailViewModel
    {
        [Display(Name = "Nuova Email"), Required, EmailAddress]
        public string NewEmail { get; set; }

        public string Email { get; set; }
        public bool IsEmailConfirmed { get; set; }
    }
}