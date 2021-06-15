using System.Collections.Generic;
using System.Linq;
using IdentityServer.Models;

namespace IdentityServer.ViewModels.Account
{
    public record LoginViewModel : LoginInputModel
    {
        public LoginViewModel(bool allowRememberLogin = true, bool enableLocalLogin = true, IEnumerable<ExternalProvider>? externalProviders = default)
        {
            AllowRememberLogin = allowRememberLogin;
            EnableLocalLogin = enableLocalLogin;
            ExternalProviders = externalProviders?.ToHashSet() ?? new HashSet<ExternalProvider>();
            
            IsExternalLoginOnly = !EnableLocalLogin && ExternalProviders.Count == 1;
            ExternalLoginScheme = IsExternalLoginOnly ? ExternalProviders.SingleOrDefault()?.AuthenticationScheme : default;
            VisibleExternalProviders = ExternalProviders.Where(x => !string.IsNullOrWhiteSpace(x.DisplayName)).ToHashSet();
        }
        
        public IReadOnlyCollection<ExternalProvider> ExternalProviders { get; }
        public IReadOnlyCollection<ExternalProvider> VisibleExternalProviders { get; }
        
        public bool AllowRememberLogin { get; } = true;
        public bool EnableLocalLogin { get; } = true;
        public bool IsExternalLoginOnly { get; }
        public string? ExternalLoginScheme { get; }
    }
}