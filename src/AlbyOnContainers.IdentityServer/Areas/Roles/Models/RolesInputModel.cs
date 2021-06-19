using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Areas.Roles.Models
{
    public record RolesInputModel
    {
        [Required(ErrorMessage = "The Role field is required."), MinLength(4)]
        public string SelectedRole { get; init; } = string.Empty;
    }
}