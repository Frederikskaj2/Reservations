using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record LockBoxCodesEmail(
    EmailAddress Email,
    string FullName,
    OrderId OrderId,
    ResourceId ResourceId,
    LocalDate Date,
    Seq<DatedLockBoxCode> Codes);
