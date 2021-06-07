using System.Collections.Generic;

namespace IdentityServer.Models.RolesViewModels
{
    public class UpdateUserRoleViewModel
    {
        public IEnumerable<UsersViewModel> Users { get; set; } = new List<UsersViewModel>();
        public IEnumerable<string> Roles { get; set; } = new List<string>();

        public string? UserEmail { get; set; }
        public string? CurrentRole { get; set; }
        public string? UpsertRole { get; set; }
    }
}