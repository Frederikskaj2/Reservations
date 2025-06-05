using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using static Frederikskaj2.Reservations.Orders.AccountNumberFunctions;
using static Frederikskaj2.Reservations.Orders.CancelResidentReservations;

namespace Frederikskaj2.Reservations.Orders;

static class UpdateMyOrder
{
    public static Either<Failure<Unit>, UpdateMyOrderOutput> UpdateMyOrderCore(
        OrderingOptions options, ITimeConverter timeConverter, UpdateMyOrderInput input) =>
        from cancelResidentReservationsOutput in CancelResidentReservationsCore(options, timeConverter, GetCancelResidentReservationsInput(input))
        select new UpdateMyOrderOutput(
            UpdateAccountNumber(cancelResidentReservationsOutput.User, input.Command.Timestamp, input.Command.UserId, input.Command.AccountNumber),
            cancelResidentReservationsOutput.Order,
            cancelResidentReservationsOutput.TransactionOption,
            IsUserDeletionConfirmed(cancelResidentReservationsOutput.User));

    static CancelResidentReservationsInput GetCancelResidentReservationsInput(UpdateMyOrderInput input) =>
        GetCancelResidentReservationsInput(input, input.Command.Timestamp <= input.Order.Specifics.Resident.NoFeeCancellationIsAllowedBefore);

    static CancelResidentReservationsInput GetCancelResidentReservationsInput(UpdateMyOrderInput input, bool alwaysAllowCancellation) =>
        new(
            input.Command.Timestamp,
            input.Command.UserId,
            input.Command.CancelledReservations,
            alwaysAllowCancellation,
            alwaysAllowCancellation,
            input.User,
            input.Order,
            input.TransactionIdOption);

    static bool IsUserDeletionConfirmed(User user) =>
        user.Flags.HasFlag(UserFlags.IsPendingDelete) && user.Orders.IsEmpty && user.Balance() == Amount.Zero;
}
