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

        public sealed class Register : AccountRequests, IRequest<IResult<Unit, IdentityError>>
        {
            public Register(string username, string email, string password, string host, string returnUrl)
            {
                Username = username;
                Email = email;
                Password = password;
                Host = host;
                ReturnUrl = returnUrl;
            }

            public string Username { get; }
            public string Email { get; }
            public string Password { get; }
            public string Host { get; }
            public string ReturnUrl { get; }
        }

        public sealed class ConfirmEmail : AccountRequests, INotification
        {
            public ConfirmEmail(Guid userId, string code)
            {
                UserId = userId;
                Code = code;
            }

            public Guid UserId { get; }
            public string Code { get; }
        }

        public sealed class RecoverPassword : AccountRequests, INotification
        {
            public RecoverPassword(string email, string host, string returnUrl)
            {
                Email = email;
                Host = host;
                ReturnUrl = returnUrl;
            }

            public string Email { get; }
            public string Host { get; }
            public string ReturnUrl { get; }
        }

        public sealed class ResetPassword : AccountRequests, INotification
        {
            public ResetPassword(string email, string password, string code)
            {
                Email = email;
                Password = password;
                Code = code;
            }

            public string Email { get; }
            public string Password { get; }
            public string Code { get; }
        }

        public sealed class ResendConfirmationEmail : AccountRequests, INotification
        {
            public ResendConfirmationEmail(string email, string host, string returnUrl)
            {
                Email = email;
                Host = host;
                ReturnUrl = returnUrl;
            }

            public string Email { get; }
            public string Host { get; }
            public string ReturnUrl { get; }
        }
    }
}