using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Models.ManageViewModel
{
    public class ChangeEmailViewModel
    {
        [Required, EmailAddress, Display(Name = "Nuova Email")]
        public string NewEmail { get; set; }
        
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsEmailConfirmed { get; set; }

        public string StatusMessage { get; set; }
        
        public string ReturnUrl { get; set; }
    }
}