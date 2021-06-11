using System.Collections.Generic;
using System.Linq;

namespace IdentityServer.Areas.Roles.Models
{
    public record RolesViewModel : RolesInputModel
    {
        public IReadOnlyCollection<(string value, int index)> Roles { get; }

        public RolesViewModel(IEnumerable<string>? roles)
        {
            Roles = roles?.Select((value, index) => (value, index)).OrderBy(x => x.value).ToHashSet() ?? new HashSet<(string value, int index)>();
        }
    }
}