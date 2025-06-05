using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record SendDebtRemindersCommand(Instant Timestamp);