namespace IdentityServer.ViewModels.Account
{
    public record LogoutViewModel : LogoutInputModel
    {
        public LogoutViewModel(bool showLogoutPrompt)
        {
            ShowLogoutPrompt = showLogoutPrompt;
        }
        
        public bool  ShowLogoutPrompt { get; } = true;
    }
}