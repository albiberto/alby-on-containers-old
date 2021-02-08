using System;

namespace IdentityServer.Models.AccountViewModels
{
    public class ConfirmEmailViewModel
    {
        public Guid UserId { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
    }
}