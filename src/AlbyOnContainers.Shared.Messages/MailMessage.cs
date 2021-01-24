using System.Collections.Generic;

namespace AlbyOnContainers.Messages
{
    public class EmailMessage
    {
        public MailAddress Sender { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public ICollection<MailAddress> ReplyToList { get; } = new List<MailAddress>();
        public ICollection<MailAddress> To { get; set; }
        public ICollection<MailAddress> Bcc { get; set; }
        public ICollection<MailAddress> CC { get; set; }
    }

    public class MailAddress
    {
        public string Email { get; set; }
        public string Name { get; set; }
    }
}