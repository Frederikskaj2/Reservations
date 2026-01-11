using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record PlaceResidentOrderCommand(
    Instant Timestamp,
    UserId AdministratorId,
    UserId ResidentId,
    string FullName,
    string Phone,
    ApartmentId ApartmentId,
    AccountNumber AccountNumber,
    Seq<ReservationModel> Reservations);
