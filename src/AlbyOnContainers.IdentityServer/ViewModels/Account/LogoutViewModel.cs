namespace IdentityServer.ViewModels.Account
{
    public record LogoutViewModel : LogoutInputModel
    {
        public readonly TitleViewModel TitleModel = new("Logout", "Would you like to logout of IdentityServer?");

        public LogoutViewModel(bool showLogoutPrompt, string? logoutId)
        {
            ShowLogoutPrompt = showLogoutPrompt;
            LogoutId = logoutId;
        }

        public bool ShowLogoutPrompt { get; } = true;
    }
}