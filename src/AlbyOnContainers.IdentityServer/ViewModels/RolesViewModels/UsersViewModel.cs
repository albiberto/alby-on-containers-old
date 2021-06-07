using System.Collections.Generic;

namespace IdentityServer.ViewModels.RolesViewModels
{
    public class UsersViewModel
    {
        public string Email { get; set; }
        public IEnumerable<UsersRole> Roles { get; set; }
    }
}