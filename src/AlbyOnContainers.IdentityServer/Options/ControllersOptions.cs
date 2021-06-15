namespace IdentityServer.Options
{
    public class ControllersOptions
    {
        public AccountOptions? Account { get; set; }
    }
    
    public class AccountOptions
    {
        public bool AllowLocalLogin { get; set; }
        public bool AllowRememberLogin { get; set; }
        public bool ShowLogoutPrompt { get; set; }
        public bool AutomaticRedirectAfterSignOut { get; set; }
        public string InvalidCredentialsErrorMessage { get; set; } = "Invalid Username or Password!";
    }
}