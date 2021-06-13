using System.Collections.Generic;
using System.Linq;
using IdentityServer4.Models;

namespace IdentityServer.ViewModels
{
    public record ConsentViewModel : ConsentInputModel
    {
        const bool EnableOfflineAccess = true;
        const string Unknown = "Unknown";

        public ConsentViewModel(AuthorizationRequest request, ConsentInputModel? model = default)
        {
            RememberConsent = model?.RememberConsent ?? true;
            ScopesConsented = model?.ScopesConsented ?? new List<string>();

            ClientName = request.Client?.ClientName ?? request.Client?.ClientId ?? Unknown;
            ClientUrl = request.Client?.ClientUri ?? string.Empty;
            ClientLogoUrl = request.Client?.LogoUri ?? string.Empty;
            AllowRememberConsent = request.Client?.AllowRememberConsent ?? false;

            IdentityResources = request.ValidatedResources.Resources.IdentityResources
                .Select(x => new ScopeViewModel(x, model?.ScopesConsented?.Contains(x.Name) ?? true))
                .ToList();

            ApiScopes = (
                from parsedScope in request.ValidatedResources.ParsedScopes 
                let apiScope = request.ValidatedResources.Resources.FindApiScope(parsedScope.ParsedName) 
                where apiScope != null 
                let resources = request.ValidatedResources.Resources.FindApiResourcesByScope(apiScope.Name).ToList()
                select new ScopeViewModel(parsedScope, apiScope, resources, model?.ScopesConsented?.Contains(parsedScope.RawValue) ?? true)
            ).ToList();

            if (!EnableOfflineAccess || !request.ValidatedResources.Resources.OfflineAccess) return;
            
            var scope = new ScopeViewModel(
                IdentityServer4.IdentityServerConstants.StandardScopes.OfflineAccess,
                "Offline Access",
                "Consent offline access.",
                true, 
                false, 
                model?.ScopesConsented?.Contains(IdentityServer4.IdentityServerConstants.StandardScopes.OfflineAccess) ?? true);
            
            ApiScopes.Add(scope);
        }

        public string ClientName { get; }
        public string ClientUrl { get; }
        public string ClientLogoUrl { get; }
        public bool AllowRememberConsent { get; }

        public List<ScopeViewModel> IdentityResources { get; } = new();
        public List<ScopeViewModel> ApiScopes { get; } = new();
    }
}