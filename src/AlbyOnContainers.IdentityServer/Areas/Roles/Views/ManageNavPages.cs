using System;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IdentityServer.Areas.Roles.Views
{
    public static class ManageNavPages
    {
        public static string Roles => "Roles";
        public static string UserRoles => "UserRoles";

        public static string RolesNavClass(ViewContext viewContext) => PageNavClass(viewContext, Roles);

        public static string UserRolesNavClass(ViewContext viewContext) => PageNavClass(viewContext, UserRoles);

        static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals($"{activePage}", page, StringComparison.OrdinalIgnoreCase) ? "active" : string.Empty;
        }
    }
}