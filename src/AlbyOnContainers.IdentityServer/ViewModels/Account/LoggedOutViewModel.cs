namespace IdentityServer.ViewModels.Account
{
    public record LoggedOutViewModel(
        string? ClientName = default, string? LogoutId = default, string? PostLogoutRedirectUri = default, 
        string? SignOutIframeUrl = default, bool AutomaticRedirectAfterSignOut = true, string? ExternalAuthenticationScheme = default)
    {
        public readonly TitleViewModel ModelTitle = new("Logout", "You are now logged out.");
        public bool TriggerExternalSignOut => !string.IsNullOrEmpty(ExternalAuthenticationScheme);
    }
}