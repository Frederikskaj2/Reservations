using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;
using static Frederikskaj2.Reservations.Orders.AccountNumberFunctions;
using static Frederikskaj2.Reservations.Orders.CancelResidentReservations;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

static class UpdateResidentOrder
{
    public static Either<Failure<Unit>, UpdateResidentOrderOutput> UpdateResidentOrderCore(
        OrderingOptions options, ITimeConverter timeConverter, UpdateResidentOrderInput input) =>
        from cancelResidentReservationsOutput in CancelResidentReservationsCore(options, timeConverter, CreateCancelResidentReservationsInput(input))
        let tuple = TryAllowCancellationWithoutFee(options, input.Command, cancelResidentReservationsOutput.Order)
        select new UpdateResidentOrderOutput(
            UpdateAccountNumber(
                cancelResidentReservationsOutput.User,
                input.Command.Timestamp,
                input.Command.AdministratorId,
                input.Command.AccountNumber),
            tuple.Order,
            cancelResidentReservationsOutput.TransactionOption,
            tuple.IsCancellationWithoutFeeAllowed);

    static CancelResidentReservationsInput CreateCancelResidentReservationsInput(UpdateResidentOrderInput input) =>
        new(
            input.Command.Timestamp,
            input.Command.AdministratorId,
            input.Command.CancelledReservations,
            input.Command.WaiveFee,
            AlwaysAllowCancellation: true,
            input.User,
            input.Order,
            input.TransactionIdOption);

    static (Order Order, bool IsCancellationWithoutFeeAllowed) TryAllowCancellationWithoutFee(
        OrderingOptions options, UpdateResidentOrderCommand command, Order order) =>
        (IsCancellationWithoutFeeEnabled(command.Timestamp, order), command.AllowCancellationWithoutFee) switch
        {
            (false, true) => (AllowCancellationWithoutFee(options, command.Timestamp, command.AdministratorId, order), true),
            (true, false) => (DisallowCancellationWithoutFee(command.Timestamp, command.AdministratorId, order), false),
            _ => (order, false),
        };

    static bool IsCancellationWithoutFeeEnabled(Instant timestamp, Order order) =>
        timestamp <= order.Specifics.Resident.NoFeeCancellationIsAllowedBefore;

    static Order AllowCancellationWithoutFee(OrderingOptions options, Instant timestamp, UserId administratorId, Order order) =>
        order with
        {
            Specifics = order.Specifics.Resident with { NoFeeCancellationIsAllowedBefore = timestamp.Plus(options.CancellationWithoutFeeDuration) },
            Audits = order.Audits.Add(OrderAudit.AllowCancellationWithoutFee(timestamp, administratorId)),
        };

    static Order DisallowCancellationWithoutFee(Instant timestamp, UserId administratorId, Order order) =>
        order with
        {
            Specifics = order.Specifics.Resident with { NoFeeCancellationIsAllowedBefore = None },
            Audits = order.Audits.Add(OrderAudit.DisallowCancellationWithoutFee(timestamp, administratorId)),
        };
}
