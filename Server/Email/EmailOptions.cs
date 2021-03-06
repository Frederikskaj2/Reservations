﻿using System;
using System.Collections.Generic;
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
        public EmailRecipient? From { get; set; }
        public EmailRecipient? ReplyTo { get; set; }
        public Uri? BaseUrl { get; set; }
        public Duration ConfirmEmailUrlLifetime { get; set; } = Duration.FromDays(7);
        public TimeSpan ScheduleStartDelay { get; set; } = TimeSpan.FromMinutes(1);
        public TimeSpan ScheduleInterval { get; set; } = TimeSpan.FromHours(6);
        public IEnumerable<string>? AllowedRecipients { get; set; }
    }
}