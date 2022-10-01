using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Shared.Email;

public record MessageBase(EmailAddress Email, string FullName);
