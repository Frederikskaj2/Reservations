using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

public record UpdateUserReservationsCommand(Instant Timestamp, UserId AdministratorUserId, OrderId OrderId, Seq<ReservationUpdate> Reservations);
