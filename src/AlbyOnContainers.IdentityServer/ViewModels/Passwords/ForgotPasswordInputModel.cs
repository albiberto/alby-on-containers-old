using System.ComponentModel.DataAnnotations;

namespace IdentityServer.ViewModels.Passwords
{
    public record ForgotPasswordInputModel
    {
        [Required, EmailAddress, Display(Name = "Email")]
        public string? Email { get; init; }
    }
}