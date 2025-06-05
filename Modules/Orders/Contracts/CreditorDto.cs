using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Orders;

public record CreditorDto(UserIdentityDto UserInformation, PaymentInformation Payment);
