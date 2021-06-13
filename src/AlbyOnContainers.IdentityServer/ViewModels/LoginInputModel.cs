using System.ComponentModel.DataAnnotations;

namespace IdentityServer.ViewModels
{
    public record LoginInputModel
    {
        [Required, EmailAddress] 
        public string Email { get; set; } = null!;
        
        [Required, DataType(DataType.Password)]
        public string Password { get; init; } = null!;
        
        [Display(Name = "Ricordami?")] public bool RememberMe { get; set; }
    }
}