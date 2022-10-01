using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

record LineItem(Instant Timestamp, LineItemType Type, CancellationFee? CancellationFee, Damages? Damages, Amount Amount);
