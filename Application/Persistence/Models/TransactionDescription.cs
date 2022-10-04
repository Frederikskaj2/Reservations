using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;

namespace Frederikskaj2.Reservations.Application;

record TransactionDescription(TransactionDescriptionType Type, CancellationDescription? Cancellation, SettlementDescription? Settlement, ReservationsUpdateDescription? ReservationsUpdate)
{
    public static TransactionDescription CreateCancellationDescription(Seq<ReservedDay> cancelledReservations) =>
        new(TransactionDescriptionType.Cancellation, new(cancelledReservations), null, null);

    public static TransactionDescription CreateSettlementDescription(ReservedDay reservation, string? damages) =>
        new(TransactionDescriptionType.Settlement, null, new(reservation, damages), null);

    public static TransactionDescription CreateReservationsUpdateDescription(Seq<ReservedDay> updateReservations) =>
        new(TransactionDescriptionType.ReservationsUpdate, null, null, new(updateReservations));
}
