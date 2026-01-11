using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record PayOutNoteDto(Instant Timestamp, UserId UserId, string FullName, string Note);
