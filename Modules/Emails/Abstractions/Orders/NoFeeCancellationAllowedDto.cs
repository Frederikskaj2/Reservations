using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Emails;

public record NoFeeCancellationAllowedDto(OrderId OrderId, Duration Duration);
