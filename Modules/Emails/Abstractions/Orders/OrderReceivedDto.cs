using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Emails;

public record OrderReceivedDto(OrderId OrderId, PaymentInformation? Payment);
