namespace IdentityServer.ViewModels.Account
{
    public record LoggedOutViewModel(string? ClientName, string? LogoutId, string? PostLogoutRedirectUri, string? SignOutIframeUrl, bool AutomaticRedirectAfterSignOut, string? ExternalAuthenticationScheme)
    {
        public bool TriggerExternalSignOut => !string.IsNullOrEmpty(ExternalAuthenticationScheme);
    }
}