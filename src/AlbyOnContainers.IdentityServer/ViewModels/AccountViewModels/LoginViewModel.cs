using System.ComponentModel.DataAnnotations;

namespace IdentityServer.ViewModels.AccountViewModels
{
    public class LoginViewModel
    {
        [Required, EmailAddress] 
        public string Email { get; set; } = null!;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Display(Name = "Ricordami?")] public bool RememberMe { get; set; }
    }
}