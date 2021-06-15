using System.ComponentModel.DataAnnotations;

namespace IdentityServer.ViewModels.Passwords
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress, Display(Name = "Email")]
        public string Email { get; set; } = null!;
    }
}