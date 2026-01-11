using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record PayOutNote(Instant Timestamp, UserId UserId, string Text);
