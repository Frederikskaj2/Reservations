using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record ReimburseCommand(
    Instant Timestamp,
    UserId AdministratorUserId,
    UserId UserId,
    LocalDate Date,
    IncomeAccount AccountToDebit,
    string Description,
    Amount Amount);
