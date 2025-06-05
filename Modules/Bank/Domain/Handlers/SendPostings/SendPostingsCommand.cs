using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record SendPostingsCommand(UserId UserId, LocalDate Month);
