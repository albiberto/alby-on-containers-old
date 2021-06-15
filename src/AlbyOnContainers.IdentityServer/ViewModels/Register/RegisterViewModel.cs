using System.ComponentModel.DataAnnotations;

namespace IdentityServer.ViewModels.Register
{
    public class RegisterViewModel
    {
        [Required, Display(Name="Nome")]
        public string? GivenName { get; set; }
        
        [Required, Display(Name="Cognome")]
        public string? FamilyName { get; set; }
        
        [Display(Name = "Username")]
        public string? Username { get; set; }

        [Required, EmailAddress, Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required, StringLength(100, ErrorMessage = "La {0} deve essere lunga almeno {2} e al massimo {1} caratteri.", MinimumLength = 8), DataType(DataType.Password), Display(Name = "Password")]
        public string Password { get; set; } = null!;

        [DataType(DataType.Password), Compare("Password", ErrorMessage = "Le password inserite non coincidono."), Display(Name = "Conferma Password")]
        public string ConfirmPassword { get; set; } = null!;
    }
}