using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Orders;

public record ResidentDto(UserIdentityDto UserIdentity, PaymentId PaymentId, Amount Balance);
