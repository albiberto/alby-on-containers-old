using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Areas.Manager.Models
{
    public record DeletePersonalDataInputModel
    {
        [Required, DataType(DataType.Password)]
        public string Password { get; init; } = string.Empty;
    }
}