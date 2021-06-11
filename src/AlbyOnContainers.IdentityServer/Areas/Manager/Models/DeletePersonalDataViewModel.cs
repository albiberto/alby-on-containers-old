using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Areas.Manager.Models
{
    public class DeletePersonalDataViewModel
    {
        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }
}