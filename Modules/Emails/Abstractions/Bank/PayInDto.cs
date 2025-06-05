using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Emails;

public record PayInDto(Amount Amount, PaymentInformation? Payment);
