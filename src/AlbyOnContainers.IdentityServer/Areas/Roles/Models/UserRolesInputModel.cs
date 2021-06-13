using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Areas.Roles.Models
{
    public record UserRolesInputModel
    {
        [Required]
        public string Email { get; init; } = null!;

        public IReadOnlyCollection<string> SelectedRoles { get; init; } = new HashSet<string>();
        public IReadOnlyCollection<string> AllRoles { get; init; } = new HashSet<string>();
    }
}