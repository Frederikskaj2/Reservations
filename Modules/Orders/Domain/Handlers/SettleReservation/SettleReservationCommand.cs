using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record SettleReservationCommand(
    Instant Timestamp,
    UserId AdministratorId,
    OrderId OrderId,
    ReservationIndex ReservationIndex,
    Amount Damages,
    Option<string> Description);
