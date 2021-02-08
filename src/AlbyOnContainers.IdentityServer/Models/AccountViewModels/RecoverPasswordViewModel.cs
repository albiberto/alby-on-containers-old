using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Models.AccountViewModels
{
    public class RecoverPasswordViewModel
    {
        [Required, EmailAddress, Display(Name = "Email")]
        public string Email { get; set; }
        public string ReturnUrl { get; set; }
    }
}