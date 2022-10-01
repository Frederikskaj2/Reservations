using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

public record SettleReservationCommand(
    Instant Timestamp,
    UserId AdministratorUserId,
    UserId UserId,
    OrderId OrderId,
    ReservationId ReservationId,
    Amount Damages,
    Option<string> Description);
