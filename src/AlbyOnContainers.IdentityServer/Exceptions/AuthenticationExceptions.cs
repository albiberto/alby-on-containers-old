using System;

namespace IdentityServer.Exceptions
{
    public abstract class AuthenticationExceptions : Exception
    {
        AuthenticationExceptions(string message) : base(message)
        {
        }

        public class EmailNotFound : AuthenticationExceptions
        {
            public EmailNotFound(string message = default) : base(message)
            {
            }
        }
        
        public class UserNotFound : AuthenticationExceptions
        {
            public UserNotFound(string message = default) : base(message)
            {
            }
        }
        
        public class Generic : AuthenticationExceptions
        {
            public Generic(string message = default) : base(message)
            {
            }
        }
    }
}