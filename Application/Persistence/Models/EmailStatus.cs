using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Application;

record EmailStatus(EmailAddress Email, string NormalizedEmail, bool IsConfirmed);
