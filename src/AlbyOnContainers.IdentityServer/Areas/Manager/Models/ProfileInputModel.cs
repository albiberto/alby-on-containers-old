using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Areas.Manager.Models
{
    public record ProfileInputModel
    {
        [MinLength(3)] public string? Username { get; init; }

        [Required, Display(Name="Nome")]
        public string? GivenName { get; set; }
        
        [Required, Display(Name="Cognome")]
        public string? FamilyName { get; set; }
        
        [Phone, DisplayName("Telefono")] public string? PhoneNumber { get; init; }
        public bool PhoneNumberConfirmed { get; init; }
    }
}