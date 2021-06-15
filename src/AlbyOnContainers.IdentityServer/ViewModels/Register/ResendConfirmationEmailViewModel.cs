using System.ComponentModel.DataAnnotations;

namespace IdentityServer.ViewModels
{
    public class ResendConfirmationEmailViewModel
    {
        [Required, EmailAddress] public string Email { get; set; } = null!;
    }
}