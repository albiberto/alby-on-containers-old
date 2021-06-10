using System.ComponentModel.DataAnnotations;

namespace IdentityServer.ViewModels.AccountViewModels
{
    public record LoginInputModel
    {
        [Required, EmailAddress] 
        public string Email { get; init; } = null!;
        
        [Required, DataType(DataType.Password)]
        public string Password { get; init; } = null!;
        
        [Display(Name = "Ricordami?")] public bool RememberMe { get; init; }
    }
}