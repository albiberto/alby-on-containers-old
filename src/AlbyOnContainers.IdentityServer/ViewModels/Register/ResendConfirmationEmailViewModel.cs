using System.ComponentModel.DataAnnotations;

namespace IdentityServer.ViewModels.Register
{
    public record ResendConfirmationEmailViewModel
    {
        [Required, EmailAddress] public string Email { get; set; } = null!;
    }
}