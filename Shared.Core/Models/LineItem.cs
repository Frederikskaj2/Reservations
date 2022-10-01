using NodaTime;

namespace Frederikskaj2.Reservations.Shared.Core;

public record LineItem(Instant Timestamp, LineItemType Type, CancellationFee? CancellationFee, Damages? Damages, Amount Amount);
