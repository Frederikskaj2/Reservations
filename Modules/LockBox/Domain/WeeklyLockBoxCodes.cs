using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.LockBox;

public record WeeklyLockBoxCodes(int WeekNumber, LocalDate Date, Seq<WeeklyLockBoxCode> Codes);
