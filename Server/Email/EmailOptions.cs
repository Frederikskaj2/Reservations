using System;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class EmailOptions
    {
        public string? SendGridApiKey { get; set; }
        public EmailRecipient? From { get; set; }
        public EmailRecipient? ReplyTo { get; set; }
        public Uri? BaseUrl { get; set; }
        public Duration ConfirmEmailUrlLifetime { get; set; } = Duration.FromDays(7);
        public TimeSpan ScheduleStartDelay { get; set; } = TimeSpan.FromMinutes(1);
        public TimeSpan ScheduleInterval { get; set; } = TimeSpan.FromHours(6);
    }
}