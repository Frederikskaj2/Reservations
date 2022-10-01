using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Application;

public record PayOutEmailModel(EmailAddress Email, string FullName, Amount Amount);
