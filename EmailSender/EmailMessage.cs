using System.Collections.Generic;

namespace Frederikskaj2.Reservations.EmailSender;

record EmailMessage(string From, string? ReplyTo, IEnumerable<string> To, string Subject, string Body);
