using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record PlaceOwnerOrderCommand(Instant Timestamp, UserId UserId, string Description, Seq<ReservationModel> Reservations, bool IsCleaningRequired);
