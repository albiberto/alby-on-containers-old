using System.Collections.Generic;
using System.Linq;

namespace IdentityServer.Areas.Roles.Models
{
    public record RolesViewModel : RolesInputModel
    {
        public RolesViewModel(IEnumerable<string>? roles)
        {
            Roles = roles?.OrderBy(x => x)?.ToHashSet() ?? new HashSet<string>();
        }

        public IReadOnlyCollection<string> Roles { get; } = new List<string>();
    }
}