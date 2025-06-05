using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

record CancelResidentReservationsInput(
    Instant Timestamp,
    UserId AdministratorId,
    HashSet<ReservationIndex> CancelledReservations,
    bool WaiveFee,
    bool AlwaysAllowCancellation,
    User User,
    Order Order,
    Option<TransactionId> TransactionIdOption);
