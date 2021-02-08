using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Models.AccountViewModels
{
    public class LoginViewModel
    {
        [Required, EmailAddress, Display(Name = "Email")]
        public string Email { get; set; }

        [Required, DataType(DataType.Password), Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Ricordami?")] public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }
    }
}