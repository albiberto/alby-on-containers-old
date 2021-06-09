using System.Collections.Generic;

namespace IdentityServer.ViewModels.ConsentViewModels
{
    public record ConsentInputModel
    {
        public IReadOnlyCollection<string> ScopesConsented { get; set; } = new List<string>();
        public bool RememberConsent { get; set; }
        public string? Description { get; set; }
    }
}