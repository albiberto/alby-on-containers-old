using System;
using System.IO;
using IdentityServer.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IdentityServer.Areas.Roles.Views
{
    public static class ManageNavPages
    {
        public static string RolesNavClass(ViewContext viewContext) => PageNavClass(viewContext, TitleViewModel.Roles.Title);
        public static string UserRolesNavClass(ViewContext viewContext) => PageNavClass(viewContext, TitleViewModel.UserRoles.Title);

        static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals($"{activePage}", page, StringComparison.OrdinalIgnoreCase) ? "active" : string.Empty;
        }
    }
}