using System.ComponentModel.DataAnnotations;

namespace IdentityServer.ViewModels.ManageViewModel
{
    public class DeletePersonalDataViewModel
    {
        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }
}