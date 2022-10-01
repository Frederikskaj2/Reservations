using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Application;

public record PostingsForMonthEmailModel(EmailAddress Email, string FullName, LocalDate Month, IEnumerable<Posting> Postings);
