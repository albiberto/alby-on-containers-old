using System.ComponentModel.DataAnnotations;

namespace IdentityServer.ViewModels.Register
{
    public record ResendEmailInputModel
    {
        [Required, EmailAddress] public string? Email { get; init; }
    }
}