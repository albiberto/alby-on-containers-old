using System.ComponentModel.DataAnnotations;

namespace IdentityServer.ViewModels.AccountViewModels
{
    public class ResendConfirmationEmailViewModel
    {
        [Required, EmailAddress] public string Email { get; set; } = null!;
    }
}