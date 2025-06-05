using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record PlaceMyOrderCommand(
    Instant Timestamp,
    UserId UserId,
    string FullName,
    string Phone,
    ApartmentId ApartmentId,
    string AccountNumber,
    Seq<ReservationModel> Reservations);
