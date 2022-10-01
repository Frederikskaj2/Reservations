using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;

namespace Frederikskaj2.Reservations.Application;

record TransactionDescription(TransactionDescriptionType Type, CancellationDescription? Cancellation, SettlementDescription? Settlement)
{
    public static TransactionDescription CreateCancellationDescription(Seq<ReservedDay> cancelledReservations) =>
        new(TransactionDescriptionType.Cancellation, new(cancelledReservations), null);

    public static TransactionDescription CreateSettlementDescription(ReservedDay reservation, string? damages) =>
        new(TransactionDescriptionType.Settlement, null, new(reservation, damages));
}
