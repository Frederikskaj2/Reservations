using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

public record ReimburseCommand(
    Instant Timestamp,
    UserId AdministratorUserId,
    UserId UserId,
    LocalDate Date,
    IncomeAccount AccountToDebit,
    string Description,
    Amount Amount);
