using System.ComponentModel.DataAnnotations;

namespace IdentityServer.ViewModels.Passwords
{
    public record ResetPasswordInputModel
    {
        [Required, Display(Name = "Email"), EmailAddress]
        public string? Email { get; init; }

        [Required, Display(Name = "Password"), StringLength(100, ErrorMessage = "La {0} deve essere lunga almeno {2} e al massimo {1} caratteri.", MinimumLength = 8), DataType(DataType.Password)]
        public string? Password { get; init; }

        [Required, Display(Name = "Conferma Password"), DataType(DataType.Password), Compare("Password", ErrorMessage = "Le password inserite non coincidono.")]
        public string? ConfirmPassword { get; init; }
        [Required] public string? Code { get; init; }
    }
}