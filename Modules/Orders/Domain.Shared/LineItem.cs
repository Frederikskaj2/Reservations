using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record LineItem(Instant Timestamp, LineItemType Type, CancellationFee? CancellationFee, Damages? Damages, Amount Amount);
