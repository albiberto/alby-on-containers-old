// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer.ViewModels.ConsentViewModels
{
    public class ConsentViewModel : ConsentInputModel
    {
        public string? ClientName { get; set; }
        public string? ClientUrl { get; set; }
        public string? ClientLogoUrl { get; set; }
        public bool AllowRememberConsent { get; set; } = true;

        public List<ScopeViewModel> IdentityScopes { get; } = new List<ScopeViewModel>();
        public List<ScopeViewModel> ApiScopes { get; } = new List<ScopeViewModel>();
        public List<Claim> Claims { get; } = new List<Claim>();
    }
}
