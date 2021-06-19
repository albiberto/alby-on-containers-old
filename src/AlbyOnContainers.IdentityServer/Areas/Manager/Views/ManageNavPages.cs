using System;
using System.IO;
using IdentityServer.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IdentityServer.Areas.Manager.Views
{
    public static class ManageNavPages
    {
        
        public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, TitleViewModel.Profile.Title);

        public static string EmailNavClass(ViewContext viewContext) => PageNavClass(viewContext, TitleViewModel.ChangeEmail.Title);

        public static string ChangePasswordNavClass(ViewContext viewContext) => PageNavClass(viewContext, TitleViewModel.ChangePassword.Title);

        public static string PersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, TitleViewModel.PersonalData.Title);
        
        static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);

            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : string.Empty;
        }
    }
}