using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record AddPayOutNoteCommand(Instant Timestamp, PayOutId PayOutId, UserId UserId, string Text);
