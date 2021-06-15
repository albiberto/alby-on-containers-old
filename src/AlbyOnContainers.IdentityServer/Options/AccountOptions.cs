namespace IdentityServer.Options
{
    public class AccountOptions
    {
        public bool AllowLocalLogin { get; set; } = true;
        public bool AllowRememberLogin { get; set; } = true;
        public bool ShowLogoutPrompt { get; set; } = true;
        public bool AutomaticRedirectAfterSignOut { get; set; }
        public string InvalidCredentialsErrorMessage { get; set; } = "Invalid Username or Password!";
    }
}