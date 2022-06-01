using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class EmailMessage
    {
        public EmailMessage(string from, string? replyTo, IEnumerable<string> to, string subject, string body)
        {
            From = from;
            ReplyTo = replyTo;
            To = to;
            Subject = subject;
            Body = body;
        }

        public string From { get; }
        public string? ReplyTo { get; }
        public IEnumerable<string> To { get; }
        public string Subject { get; }
        public string Body { get; }
    }
}