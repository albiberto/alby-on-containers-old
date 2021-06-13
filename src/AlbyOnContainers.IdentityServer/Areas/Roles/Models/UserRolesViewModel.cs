using System;
using System.Collections.Generic;
using System.Linq;
using IdentityServer.Models;

namespace IdentityServer.Areas.Roles.Models
{
    public record UserRolesViewModel : UserRolesInputModel
    {
        public string Name { get; }
        public string Surname { get; }
        public RoleViewModel[] Roles { get; init; }

        public UserRolesViewModel(ApplicationUser? user = default, IEnumerable<RoleViewModel>? roles = default)
        {
            Name = user?.GivenName ?? string.Empty;
            Surname = user?.FamilyName ?? string.Empty;
            Roles = roles?.ToArray() ?? Array.Empty<RoleViewModel>();
            Email = user?.Email ?? user?.Id ?? string.Empty;
        }
    }
}