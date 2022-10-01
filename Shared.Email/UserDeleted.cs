using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Shared.Email;

public record UserDeleted(EmailAddress Email, string FullName) : MessageBase(Email, FullName);
