using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Models.ManageViewModel
{
    public class ChangeEmailViewModel
    {
        [Display(Name = "Nuova Email"), Required, EmailAddress]
        public string NewEmail { get; set; } = string.Empty;

        [Required] public string Email { get; set; } = null!;

        public bool IsEmailConfirmed { get; set; }
    }
}