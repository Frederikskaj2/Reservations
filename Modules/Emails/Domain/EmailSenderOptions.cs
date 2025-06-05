using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Frederikskaj2.Reservations.Emails;

public class EmailSenderOptions
{
    public bool IsEnabled { get; set; }

    [SuppressMessage(
        "Meziantou", "MA0016:Prefer using collection abstraction instead of implementation",
        Justification = "To initialize the set with a specific string comparer a collection abstraction cannot be used.")]
    public HashSet<string> AllowedRecipients { get; } = new(StringComparer.OrdinalIgnoreCase);
}
