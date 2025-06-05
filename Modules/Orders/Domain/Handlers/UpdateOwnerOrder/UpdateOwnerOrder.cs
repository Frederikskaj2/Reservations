using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;
using static Frederikskaj2.Reservations.Orders.CancelReservationFunctions;

namespace Frederikskaj2.Reservations.Orders;

static class UpdateOwnerOrder
{
    public static Either<Failure<Unit>, UpdateOwnerOrderOutput> UpdateOwnerOrderCore(
        OrderingOptions options, ITimeConverter timeConverter, UpdateOwnerOrderInput input) =>
        from _1 in ValidateCancelledReservations(input.Command.CancelledReservations, input.Order)
        let date = timeConverter.GetDate(input.Command.Timestamp)
        from _2 in ValidateReservationsCanBeCancelled(options, input.Command.CancelledReservations, alwaysAllowCancellation: true, input.Order, date)
        select new UpdateOwnerOrderOutput(
            TryUpdateDescription(
                input.Command,
                TryUpdateCleaning(
                    input.Command,
                    TryCancelReservations(input.Command, input.Order))));

    static Order TryUpdateDescription(UpdateOwnerOrderCommand command, Order order) =>
        command.Description.Case switch
        {
            string description => TryUpdateDescription(command.Timestamp, command.UserId, description, order),
            _ => order,
        };

    static Order TryUpdateDescription(Instant timestamp, UserId userId, string description, Order order) =>
        description != order.Specifics.Owner.Description ? UpdateDescription(timestamp, userId, description, order) : order;

    static Order UpdateDescription(Instant timestamp, UserId userId, string description, Order order) =>
        order with
        {
            Specifics = new Owner(new(description)),
            Audits = order.Audits.Add(OrderAudit.UpdateDescription(timestamp, userId)),
        };

    static Order TryUpdateCleaning(UpdateOwnerOrderCommand command, Order order) =>
        command.IsCleaningRequired.Case switch
        {
            bool isCleaningRequired => TryUpdateCleaning(command.Timestamp, command.UserId, isCleaningRequired, order),
            _ => order,
        };

    static Order TryUpdateCleaning(Instant timestamp, UserId userId, bool isCleaningRequired, Order order) =>
        isCleaningRequired ^ order.Flags.HasFlag(OrderFlags.IsCleaningRequired)
            ? UpdateCleaning(timestamp, userId, isCleaningRequired, order)
            : order;

    static Order UpdateCleaning(Instant timestamp, UserId userId, bool isCleaningRequired, Order order) =>
        order with
        {
            Flags = GetFlags(order.Flags, isCleaningRequired),
            Audits = order.Audits.Add(OrderAudit.UpdateCleaning(timestamp, userId)),
        };

    static OrderFlags GetFlags(OrderFlags flags, bool isCleaningRequired) =>
        isCleaningRequired ? flags | OrderFlags.IsCleaningRequired : flags & ~OrderFlags.IsCleaningRequired;

    static Order TryCancelReservations(UpdateOwnerOrderCommand command, Order order) =>
        !command.CancelledReservations.IsEmpty
            ? CancelReservations(command.Timestamp, command.UserId, command.CancelledReservations, order)
            : order;

    static Order CancelReservations(Instant timestamp, UserId userId, HashSet<ReservationIndex> cancelledReservations, Order order) =>
        order with
        {
            Reservations = order.Reservations
                .Map((index, reservation) => CancelReservation(cancelledReservations, reservation, ReservationIndex.FromInt32(index))).ToSeq(),
            Audits = order.Audits.Add(OrderAudit.CancelOwnerReservation(timestamp, userId)),
        };

    static Reservation CancelReservation(HashSet<ReservationIndex> cancelledReservations, Reservation reservation, ReservationIndex indexToUpdate) =>
        cancelledReservations.Find(index => index == indexToUpdate).Case switch
        {
            ReservationIndex => reservation with
            {
                Status = ReservationStatus.Cancelled,
            },
            _ => reservation,
        };
}
