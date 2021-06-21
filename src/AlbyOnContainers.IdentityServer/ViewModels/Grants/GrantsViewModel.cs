using System.Collections.Generic;
using System.Linq;

namespace IdentityServer.Areas.Grants.Models
{
    public record GrantsViewModel
    {
        public GrantsViewModel(IEnumerable<GrantViewModel>? grants)
        {
            Grants = grants?.ToHashSet() ?? new HashSet<GrantViewModel>();
        }

        public IReadOnlyCollection<GrantViewModel> Grants { get; }
    }
}