using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress, Display(Name = "Email")]
        public string Email { get; set; } = null!;
    }
}