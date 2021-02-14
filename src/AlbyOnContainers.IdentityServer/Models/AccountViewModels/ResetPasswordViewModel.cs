using System;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Models.AccountViewModels
{
    public class ResetPasswordViewModel
    {
        [Required, Display(Name = "Email"), EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Display(Name = "Password"), StringLength(maximumLength: 100, ErrorMessage = "La {0} deve essere lunga almeno {2} e al massimo {1} caratteri.", MinimumLength = 8), DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required, Display(Name = "Conferma Password"), DataType(DataType.Password), Compare("Password", ErrorMessage = "Le password inserite non coincidono.")]
        public string ConfirmPassword { get; set; } = string.Empty;
        public string Code { get; set; }
    }
}