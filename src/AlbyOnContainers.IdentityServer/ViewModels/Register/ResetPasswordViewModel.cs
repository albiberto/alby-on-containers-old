using System.ComponentModel.DataAnnotations;

namespace IdentityServer.ViewModels.Register
{
    public record ResetPasswordViewModel
    {
        [Required, Display(Name = "Email"), EmailAddress]
        public string Email { get; set; } = null!;

        [Required, Display(Name = "Password"), StringLength(100, ErrorMessage = "La {0} deve essere lunga almeno {2} e al massimo {1} caratteri.", MinimumLength = 8), DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required, Display(Name = "Conferma Password"), DataType(DataType.Password), Compare("Password", ErrorMessage = "Le password inserite non coincidono.")]
        public string ConfirmPassword { get; set; } = null!;
        [Required] public string Code { get; set; } = null!;
    }
}