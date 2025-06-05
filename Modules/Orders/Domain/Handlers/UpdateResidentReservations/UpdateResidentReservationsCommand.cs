using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record UpdateResidentReservationsCommand(Instant Timestamp, UserId AdministratorId, OrderId OrderId, Seq<ReservationUpdate> Reservations);
