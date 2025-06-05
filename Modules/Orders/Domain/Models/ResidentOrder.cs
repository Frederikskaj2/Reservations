using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record ResidentOrder(
    OrderId OrderId,
    Instant CreatedTimestamp,
    Seq<ResidentReservation> Reservations,
    bool IsHistoryOrder,
    bool CanBeEdited,
    Price Price,
    Option<Instant> NoFeeCancellationIsAllowedBefore,
    Option<PaymentInformation> PaymentInformation,
    Seq<LineItem> AdditionalLineItems,
    User User);