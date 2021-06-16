using System.Collections.Generic;
using System.Linq;
using IdentityServer.Models;

namespace IdentityServer.ViewModels.Account
{
    public record LoginViewModel : LoginInputModel
    {
        public readonly TitleViewModel Title = new("Login", "Inserisci le tue credenziali per accedere.");

        public LoginViewModel(string? email, bool? rememberLogin, bool allowRememberLogin = true, bool enableLocalLogin = true, IEnumerable<ExternalProvider>? externalProviders = default)
        {
            Email = email;
            RememberLogin = rememberLogin ?? false;

            EnableLocalLogin = enableLocalLogin;
            AllowRememberLogin = allowRememberLogin;

            var providers = externalProviders?.ToHashSet() ?? new HashSet<ExternalProvider>();

            IsExternalLoginOnly = !EnableLocalLogin && providers.Count == 1;
            ExternalLoginScheme = IsExternalLoginOnly ? providers.SingleOrDefault()?.AuthenticationScheme : default;
            VisibleExternalProviders = providers.Where(x => !string.IsNullOrWhiteSpace(x.DisplayName)).ToHashSet();
        }

        public bool EnableLocalLogin { get; }
        public bool AllowRememberLogin { get; }

        public bool IsExternalLoginOnly { get; }
        public string? ExternalLoginScheme { get; }
        public IReadOnlyCollection<ExternalProvider> VisibleExternalProviders { get; } = new HashSet<ExternalProvider>();
    }
}