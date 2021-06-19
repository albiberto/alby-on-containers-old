using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Areas.Manager.Models
{
    public record ChangeEmailInputModel
    {
        [Display(Name = "Nuova Email"), Required, EmailAddress]
        public string NewEmail { get; set; } = string.Empty;

        [Required] public string Email { get; set; } = null!;
    }
}