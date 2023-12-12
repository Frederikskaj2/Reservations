using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

public record PlaceUserOrderCommand(
    Instant Timestamp,
    UserId AdministratorUserId,
    UserId UserId,
    string FullName,
    string Phone,
    ApartmentId ApartmentId,
    string AccountNumber,
    Seq<ReservationModel> Reservations);
