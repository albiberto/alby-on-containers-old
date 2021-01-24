using System.Collections.Generic;

namespace IdentityServer.Senders
{
    public class EmailMessage
    {
        public EmailMessage()
        {
            ToAddresses = new List<EmailAddress>();
            FromAddresses = new List<EmailAddress>();
        }

        public List<EmailAddress> ToAddresses { get; set; }
        public List<EmailAddress> FromAddresses { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
    }

    public readonly struct EmailAddress
    {
        public EmailAddress(string name, string address)
        {
            Name = name;
            Address = address;
        }

        public string Name { get; }
        public string Address { get; }
    }
}