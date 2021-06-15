using System.ComponentModel.DataAnnotations;

namespace IdentityServer.ViewModels.Account
{
    public record LoginInputModel
    {
        [Required, EmailAddress] 
        public string? Email { get; set; }
        
        [Required, DataType(DataType.Password)]
        public string? Password { get; init; }

        [Display(Name = "Ricordami?")] public bool RememberLogin { get; set; } = true;
    }
}