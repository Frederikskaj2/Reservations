using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record Resident(Option<Instant> NoFeeCancellationIsAllowedBefore, Seq<LineItem> AdditionalLineItems);
