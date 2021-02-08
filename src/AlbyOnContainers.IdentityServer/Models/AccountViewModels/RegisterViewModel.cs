using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Models.AccountViewModels
{
    public class RegisterViewModel
    {
        [Required, EmailAddress, Display(Name = "Email")]
        public string Email { get; set; }

        [Required, StringLength(100, ErrorMessage = "La {0} deve essere lunga almeno {2} e al massimo {1} caratteri.", MinimumLength = 8), DataType(DataType.Password), Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password), Compare("Password", ErrorMessage = "Le password inserite non coincidono."), Display(Name = "Conferma Password")]
        public string ConfirmPassword { get; set; }
    }
}