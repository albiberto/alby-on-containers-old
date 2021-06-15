using System.ComponentModel.DataAnnotations;

namespace IdentityServer.ViewModels.Register
{
    public class ResendConfirmationEmailViewModel
    {
        [Required, EmailAddress] public string Email { get; set; } = null!;
    }
}