using System;
using MailKit.Security;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class EmailOptions
    {
        public string? SmtpHostName { get; set; }
        public int SmtpPort { get; set; }
        public SecureSocketOptions SocketOptions { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? FromName { get; set; }
        public string? FromEmail { get; set; }
        public string? ReplyToName { get; set; }
        public string? ReplyToEmail { get; set; }
        public Uri? BaseUrl { get; set; }
        public Duration ConfirmEmailUrlLifetime { get; set; } = Duration.FromDays(7);
        public string? CleaningCompanyName { get; set; }
        public string? CleaningCompanyEmail { get; set; }
    }
}