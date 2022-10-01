using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

record UserOrder(ApartmentId ApartmentId, Instant? NoFeeCancellationIsAllowedBefore, Seq<LineItem> AdditionalLineItems);
