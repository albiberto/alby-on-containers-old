using System.ComponentModel.DataAnnotations;

namespace IdentityServer.ViewModels.Account
{
    public record LoginInputModel
    {
        [Required, EmailAddress] 
        public string? Email { get; init; }
        
        [Required, DataType(DataType.Password)]
        public string? Password { get; init; }

        [Display(Name = "Ricordami?")] public bool RememberLogin { get; init; }
    }
}