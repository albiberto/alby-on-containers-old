using System;
using System.IO;
using IdentityServer.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IdentityServer.Areas.Diagnostics.Views
{
    public static class ManageNavPages
    {
        public static string DiagnosticsNavClass(ViewContext viewContext) => PageNavClass(viewContext, TitleViewModel.Diagnostics.Title);
        
        static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);

            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : string.Empty;
        }
    }
}