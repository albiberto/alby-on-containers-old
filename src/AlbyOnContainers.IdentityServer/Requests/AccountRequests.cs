using System;
using System.ComponentModel.DataAnnotations;
using IdentityServer.Models;
using MediatR;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Identity;
using Org.BouncyCastle.Asn1.Ocsp;

namespace IdentityServer.Requests
{
    public abstract class AccountRequests
    {
        public sealed class Login : AccountRequests, IRequest<IResult<Unit, Unit>>
        {
            [Required, EmailAddress] public string Email { get; set; }

            [Required, DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")] public bool RememberMe { get; set; }

            public string ReturnUrl { get; set; }
        }
        
        public sealed class Logout : AccountRequests
        {
            public string LogoutId { get; set; }
        }

        public sealed class Register : AccountRequests, IRequest<IResult<Unit, IdentityError>>
        {
            [Required, EmailAddress, Display(Name = "Email")]
            public string Email { get; set; }

            [Required, StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6), DataType(DataType.Password), Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password), Display(Name = "Confirm password"), Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
            
            public string Host { get; set; }
            public string ReturnUrl { get; set; }
        }
        public sealed class ConfirmEmail : AccountRequests, IRequest<IResult<Unit, Unit>>
        {
            [Required]
            public Guid UserId { get; set; }
            
            [Required]
            public string Code { get; set; }
            
            public string ReturnUrl { get; set; }
        }
        public sealed class ForgotPassword : AccountRequests, IRequest<IResult<Unit, IdentityError>>
        {
            [Required, EmailAddress, Display(Name = "Email")]
            public string Email { get; set; }
            public string Host { get; set; }
            public string ReturnUrl { get; set; }
        }

        public class ResetPassword : AccountRequests, IRequest<IResult<Unit, IdentityError>>
        {
            [Required, EmailAddress, Display(Name = "Email")]
            public string Email { get; set; }
            [Required, StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6), DataType(DataType.Password), Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password), Display(Name = "Confirm password"), Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
            
            [Required]
            public string Code { get; set; }
        }
        
        public class PostResetPassword : AccountRequests, IRequest<IResult<Unit, IdentityError>>
        {
            [Required, EmailAddress, Display(Name = "Email")]
            public string Email { get; set; }
            [Required, StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6), DataType(DataType.Password), Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password), Display(Name = "Confirm password"), Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
            
            [Required]
            public string Code { get; set; }
        }
    }
}