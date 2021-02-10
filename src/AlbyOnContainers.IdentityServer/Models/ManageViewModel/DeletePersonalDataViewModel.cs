using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Models.ManageViewModel
{
    public class DeletePersonalDataViewModel
    {
        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }
}