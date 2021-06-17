using IdentityServer.ViewModels.Shared;

namespace IdentityServer.ViewModels.Account
{
    public record LogoutViewModel : LogoutInputModel
    {
        public LogoutViewModel(bool showLogoutPrompt, string? logoutId)
        {
            ShowLogoutPrompt = showLogoutPrompt;
            LogoutId = logoutId;
        }

        public bool ShowLogoutPrompt { get; } = true;
    }
}