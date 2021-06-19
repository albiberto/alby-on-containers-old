namespace IdentityServer.Areas.Manager.Models
{
    public record ChangeEmailViewModel : ChangeEmailInputModel
    {
        public ChangeEmailViewModel(ChangeEmailInputModel model, bool isEmailConfirmed = false) : this(model.Email, model.NewEmail, isEmailConfirmed)
        {
        }

        public ChangeEmailViewModel(string? email = default, string? newEmail = default, bool isEmailConfirmed = false)
        {
            Email = email ?? string.Empty;
            NewEmail = newEmail ?? string.Empty;
            IsEmailConfirmed = isEmailConfirmed;
        }
        
        public bool IsEmailConfirmed { get; }
    }
}