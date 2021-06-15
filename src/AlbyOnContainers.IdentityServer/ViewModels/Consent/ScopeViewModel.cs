using System.Collections.Generic;
using System.Linq;
using IdentityServer4.Models;
using IdentityServer4.Validation;

namespace IdentityServer.ViewModels.Consent
{
    public record ScopeViewModel
    {
        const string Unknown = "Unknown";

        public ScopeViewModel(string? name,string? displayName, string? description, bool emphasize = true, bool required = false, bool @checked = false)
        {
            Name = name ?? Unknown;
            DisplayName = displayName ?? Unknown;
            Description = description ?? string.Empty;
            Emphasize = emphasize;
            Required = required;
            Checked = @checked;
        }
        
        public ScopeViewModel(IdentityResource identity, bool check) : this(identity.Name, identity.DisplayName, identity.Description, identity.Emphasize, identity.Required, check || identity.Required)
        {
        }
        
        public ScopeViewModel(ParsedScopeValue? parsedScopeValue, ApiScope? apiScope, IReadOnlyCollection<Resource> resources, bool check)
        {
            var displayName = apiScope?.DisplayName ?? apiScope?.Name ?? Unknown;
            if (!string.IsNullOrWhiteSpace(parsedScopeValue?.ParsedParameter)) displayName += ":" + parsedScopeValue.ParsedParameter;

            Name = parsedScopeValue?.RawValue ?? Unknown;
            DisplayName = displayName;
            Description = apiScope?.Description ?? string.Empty;
            Emphasize = apiScope?.Emphasize ?? false;
            Required = apiScope?.Required ?? false;
            Checked = check || (apiScope?.Required ?? false);

            var apiResources = resources.Select(r => r.Name).ToList();
            ApiResources = string.Join(", ", apiResources);
        }

        public string Name { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public bool Emphasize { get; }
        public bool Required { get; }
        public bool Checked { get; }
        public string?  ApiResources { get; }
    }
}