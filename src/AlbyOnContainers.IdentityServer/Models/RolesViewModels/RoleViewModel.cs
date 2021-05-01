using System.Collections.Generic;

namespace IdentityServer.Models.RolesViewModels
{
    public class RoleViewModel
    {
        public IEnumerable<string> Roles { get; set; }
        public string Name { get; set; }
        public bool Delete { get; set; }
    }
}