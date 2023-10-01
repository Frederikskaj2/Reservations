using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;

namespace Frederikskaj2.Reservations.Application;

record TransactionDescription(
    TransactionDescriptionType Type,
    CancellationDescription? Cancellation,
    SettlementDescription? Settlement,
    ReservationsUpdateDescription? ReservationsUpdate,
    ReimbursementDescription? Reimbursement)
{
    public static TransactionDescription CreateCancellationDescription(Seq<ReservedDay> cancelledReservations) =>
        new(TransactionDescriptionType.Cancellation, new(cancelledReservations), null, null, null);

    public static TransactionDescription CreateSettlementDescription(ReservedDay reservation, string? damages) =>
        new(TransactionDescriptionType.Settlement, null, new(reservation, damages), null, null);

    public static TransactionDescription CreateReservationsUpdateDescription(Seq<ReservedDay> updateReservations) =>
        new(TransactionDescriptionType.ReservationsUpdate, null, null, new(updateReservations), null);

    public static TransactionDescription CreateReimbursementDescription(IncomeAccount accountToDebit, string description) =>
        new(TransactionDescriptionType.Reimbursement, null, null, null, new(accountToDebit, description));
}
