using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record ReimburseRequest(LocalDate Date, IncomeAccount AccountToDebit, string? Description, Amount Amount);
