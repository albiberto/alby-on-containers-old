using System;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer.Areas.Grants.Models
{
    public class GrantViewModel
    {
        public GrantViewModel(string clientId, string? clientName, string? clientUrl, string? clientLogoUrl, string? description, DateTime created, DateTime? expires, IEnumerable<string>? identityGrantNames, IEnumerable<string>? apiGrantNames)
        {
            ClientId = clientId;
            ClientName = clientName ?? clientId;
            ClientUrl = clientUrl ?? string.Empty;
            ClientLogoUrl = clientLogoUrl ?? string.Empty;
            Description = description ?? string.Empty;
            Created = created;
            Expires = expires;
            IdentityGrantNames = identityGrantNames?.ToHashSet() ?? new HashSet<string>();
            ApiGrantNames = apiGrantNames?.ToHashSet() ?? new HashSet<string>();
        }

        public string ClientId { get; }
        public string ClientName { get; }
        public string ClientUrl { get; }
        public string ClientLogoUrl { get; }
        public string Description { get; }
        public DateTime Created { get; }
        public DateTime? Expires { get; }
        public IReadOnlyCollection<string> IdentityGrantNames { get; }
        public IReadOnlyCollection<string> ApiGrantNames { get; }
    }
}