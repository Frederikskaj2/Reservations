using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Emails;

public record EmailMessage(string From, string? ReplyTo, IReadOnlyCollection<string> To, string Subject, string Body);
