using System;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.EmailSender;

public class EmailSenderOptions
{
    public bool IsEnabled { get; set; }
    public TimeSpan PollInterval { get; init; } = TimeSpan.FromSeconds(5);
    public IEnumerable<string>? AllowedRecipients { get; init; }
}
