using System.ComponentModel.DataAnnotations;

namespace IdentityServer.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress, Display(Name = "Email")]
        public string Email { get; set; } = null!;
    }
}