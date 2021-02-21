namespace Hermes.Options
{
    public class EmailOptions
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public string DefaultSenderName { get; set; }
        public string DefaultSenderEmail { get; set; }
        public bool UseSsl { get; set; }
    }
}