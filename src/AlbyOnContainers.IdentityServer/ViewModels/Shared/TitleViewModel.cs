namespace IdentityServer.ViewModels.Shared
{
    public record TitleViewModel(string Title, string? Description = default)
    {
        public static readonly TitleViewModel Login = new("Login", "Inserisci le tue credenziali per accedere.");
        public static readonly TitleViewModel Logout = new("Logout", "Would you like to logout of IdentityServer?");
        public static readonly TitleViewModel LoggedOut = new("Logout", "You are now logged out.");

        public static readonly TitleViewModel AccessDenied = new("Access Denied", "You do not have access to that resource.");

        public static readonly TitleViewModel ForgotPassword = new("Recupera Password", "Password dimenticata? Inserisci la tua email!");
        public static readonly TitleViewModel ResetPassword = new("Ripristina Password", "Compila i seguenti campi per ripristinare la password!");
        public static readonly TitleViewModel ResetPasswordConfirmation = new("Password Modificata!", "La tua password &egrave; stata modificata!");
        
        public static readonly TitleViewModel Register = new ("Register", "Completa i seguenti campi per registrarti");
        public static readonly TitleViewModel ResendEmail = new ("Invia email di conferma", "Inserisci la tua email per continuare.");

        public static readonly TitleViewModel ChangeEmail = new("Cambio Email");
        public static readonly TitleViewModel Profile = new("Profilo");
        public static readonly TitleViewModel ChangePassword = new("Cambio Password");
        public static readonly TitleViewModel PersonalData = new("Dati Personali");
        public static readonly TitleViewModel DeletePersonalData = new("Elimina Account");
        
        public static readonly TitleViewModel Roles = new("Ruoli");
        public static readonly TitleViewModel UserRoles = new("Ruoli Utente");

        public static TitleViewModel EmailConfirmation(string? email = default)
        {
            var part = !string.IsNullOrEmpty(email)
                ? $" all'indirizzo {email}"
                : string.Empty;
            
            return new("Email Inviata", $"Ti abbiamo spedito una email{part}. Controlla la tua casella di posta elettronica.");
        }

        public static TitleViewModel ConfirmEmail(bool? success = false)
        {
            var part = success ?? false
                ? "Grazie per aver confermato la tua email."
                : "Ops... si e' verificato un errore :(";
            
            return new("Email inviata", $"Ti abbiamo spedito una email{part}. Controlla la tua casella di posta elettronica.");
        }
    }
}