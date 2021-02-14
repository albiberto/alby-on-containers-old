using System;
using IdentityServer.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Requests
{
    public abstract class AccountRequests
    {
        // public string Email { get; set; }
        // public string Password { get; set; }

        public sealed class Login : AccountRequests, IRequest<IResult<Unit, Unit>>
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public bool RememberMe { get; set; }
            public string ReturnUrl { get; set; }
        }

        public sealed class Logout : AccountRequests
        {
            public string LogoutId { get; set; }
        }
    }
}