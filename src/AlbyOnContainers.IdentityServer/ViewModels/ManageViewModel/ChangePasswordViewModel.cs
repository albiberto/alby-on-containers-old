using System.ComponentModel.DataAnnotations;

namespace IdentityServer.ViewModels.ManageViewModel
{
    public class ChangePasswordViewModel
    {
        [Required]
        [Display(Name = "Password Attuale")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required]
        [Display(Name = "Nuova Password")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Display(Name = "Conferma Nuova Password")]
        [DataType(DataType.Password), Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}