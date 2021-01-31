using System;
using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Requests
{
    public abstract class AccountRequests
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public sealed class Login : AccountRequests, IRequest<IResult<Unit, Unit>>
        {
            public bool RememberMe { get; set; }
            public string ReturnUrl { get; set; }
        }
        
        public sealed class Logout : AccountRequests
        {
            public string LogoutId { get; set; }
        }

        public sealed class Register : AccountRequests, IRequest<IResult<Unit, IdentityError>>
        {
            public string Host { get; set; }
            public string ReturnUrl { get; set; }
        }
        public sealed class ConfirmEmail : AccountRequests, IRequest<IResult<Unit, Unit>>
        {
            public Guid UserId { get; set; }
            public string Code { get; set; }
        }
        public sealed class RecoverPassword : AccountRequests, IRequest<IResult<Unit, IdentityError>>
        {
            public string Host { get; set; }
            public string ReturnUrl { get; set; }
        }

        public class ResetPassword : AccountRequests, IRequest<IResult<Unit, IdentityError>>
        {
            public Guid UserId { get; set; }
            public string Code { get; set; }
        }
        
    }
}