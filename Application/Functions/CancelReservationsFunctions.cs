using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using System.Linq;
using System.Net;
using static Frederikskaj2.Reservations.Application.AccountsReceivableFunctions;
using static Frederikskaj2.Reservations.Application.CleaningScheduleFunctions;
using static Frederikskaj2.Reservations.Application.HistoryOrderFunctions;
using static Frederikskaj2.Reservations.Application.TransactionFunctions;
using static Frederikskaj2.Reservations.Application.UserBalanceFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

static class CancelReservationsFunctions
{
    public static EitherAsync<Failure, IPersistenceContext> TryCancelReservations(
        OrderingOptions options, Instant timestamp, UserId updatedByUserId, HashSet<ReservationIndex> cancelledReservations, bool waiveFee,
        bool alwaysAllowCancellation, LocalDate date, IPersistenceContext context, Order order) =>
        !cancelledReservations.IsEmpty
            ? CancelReservations(options, timestamp, updatedByUserId, cancelledReservations, waiveFee, alwaysAllowCancellation, date, context, order)
            : RightAsync<Failure, IPersistenceContext>(context);

    static EitherAsync<Failure, IPersistenceContext> CancelReservations(
        OrderingOptions options, Instant timestamp, UserId updatedByUserId, HashSet<ReservationIndex> cancelledReservations, bool waiveFee,
        bool alwaysAllowCancellation, LocalDate date, IPersistenceContext context, Order order) =>
        from context1 in CancelReservationsCore(options, timestamp, updatedByUserId, cancelledReservations, waiveFee, alwaysAllowCancellation, date, context, order)
        let context2 = ScheduleCleaning(options, context1)
        let transaction = context2.Item<Transaction>()
        let context3 = ApplyCreditToOrders(timestamp, updatedByUserId, context2, transaction.TransactionId)
        select TryMakeHistoryOrderWhenCancelling(timestamp, updatedByUserId, order.OrderId, cancelledReservations, context3);

    static IPersistenceContext TryMakeHistoryOrderWhenCancelling(
        Instant timestamp, UserId updatedByUserId, OrderId orderId, HashSet<ReservationIndex> cancelledReservations, IPersistenceContext context) =>
        cancelledReservations.Count > 0 ? TryMakeHistoryOrder(timestamp, updatedByUserId, orderId, context) : context;

    public static EitherAsync<Failure, IPersistenceContext> TryCancelOwnerReservations(
        OrderingOptions options, Instant timestamp, UserId updatedByUserId, HashSet<ReservationIndex> cancelledReservations, LocalDate date,
        IPersistenceContext context, Order order) =>
        !cancelledReservations.IsEmpty
            ? CancelOwnerReservations(options, timestamp, updatedByUserId, cancelledReservations, date, context, order)
            : RightAsync<Failure, IPersistenceContext>(context);

    static EitherAsync<Failure, IPersistenceContext> CancelOwnerReservations(
        OrderingOptions options, Instant timestamp, UserId updatedByUserId, HashSet<ReservationIndex> cancelledReservations, LocalDate date,
        IPersistenceContext context, Order order) =>
        CancelReservationsCore(options, timestamp, updatedByUserId, cancelledReservations, true, true, date, context, order);

    static EitherAsync<Failure, IPersistenceContext> CancelReservationsCore(
        OrderingOptions options, Instant timestamp, UserId updatedByUserId, HashSet<ReservationIndex> cancelledReservations, bool waiveFee,
        bool alwaysAllowCancellation, LocalDate date, IPersistenceContext context, Order order) =>
        from _ in ValidateCancelledReservations(options, cancelledReservations, alwaysAllowCancellation, date, order)
        from context1 in AddTransactionIfNeeded(options, timestamp, updatedByUserId, cancelledReservations, waiveFee, date, context, order)
        select UpdateOrder(timestamp, updatedByUserId, cancelledReservations, order.OrderId, context1);

    static EitherAsync<Failure, Unit> ValidateCancelledReservations(
        OrderingOptions options, HashSet<ReservationIndex> cancelledReservations, bool alwaysAllowCancellation, LocalDate date, Order order) =>
        from _1 in ValidateCancelledReservations(cancelledReservations, order)
        from _2 in ValidateReservationsCanBeCancelled(options, cancelledReservations, alwaysAllowCancellation, date, order)
        select unit;

        static EitherAsync<Failure, Unit> ValidateCancelledReservations(HashSet<ReservationIndex> cancelledReservations, Order order) =>
            cancelledReservations.ForAll(index => 0 <= index && index < order.Reservations.Count)
                ? unit
                : Failure.New(HttpStatusCode.UnprocessableEntity, $"One or more cancelled reservations on order {order.OrderId} are invalid.");

        static EitherAsync<Failure, Unit> ValidateReservationsCanBeCancelled(
            OrderingOptions options, HashSet<ReservationIndex> cancelledReservations, bool alwaysAllowCancellation, LocalDate date, Order order) =>
        cancelledReservations
            .SequenceSerial(index => ValidateReservationCanBeCancelled(options, date, order.Reservations[index.ToInt32()], alwaysAllowCancellation))
            .Map(_ => unit);

    static EitherAsync<Failure, Unit> ValidateReservationCanBeCancelled(
        OrderingOptions options, LocalDate date, Reservation reservation, bool alwaysAllowCancellation) =>
        ReservationPolicies.CanReservationBeCancelled(options, date, reservation.Status, reservation.Extent, alwaysAllowCancellation)
            ? unit
            : Failure.New(HttpStatusCode.UnprocessableEntity, $"Reservation {reservation} cannot be cancelled by user.");

    static EitherAsync<Failure, IPersistenceContext> AddTransactionIfNeeded(
        OrderingOptions options, Instant timestamp, UserId userId, HashSet<ReservationIndex> cancelledReservations, bool waiveFee, LocalDate date,
        IPersistenceContext context, Order order) =>
        !order.Flags.HasFlag(OrderFlags.IsOwnerOrder) && !cancelledReservations.IsEmpty
            ? AddTransaction(options,
                timestamp,
                userId,
                waiveFee,
                cancelledReservations.OrderBy(index => index).ToSeq(),
                date,
                context,
                order)
            : RightAsync<Failure, IPersistenceContext>(context);

    static EitherAsync<Failure, IPersistenceContext> AddTransaction(
        OrderingOptions options, Instant timestamp, UserId userId, bool waiveFee, Seq<ReservationIndex> cancelledReservations, LocalDate date,
        IPersistenceContext context, Order order) =>
        from transactionId in IdGenerator.CreateId(context.Factory, nameof(Transaction))
        let fee = GetFee(options, waiveFee, order)
        let transaction = CreateCancelReservationTransaction(
            timestamp, userId, date, order, cancelledReservations.Map(index => order.Reservations[index.ToInt32()]), transactionId, fee)
        let context1 = UpdateOrderAndUser(timestamp, order.OrderId, context, cancelledReservations, fee, transaction)
        select AddTransaction(context1, transaction);

    static IPersistenceContext UpdateOrderAndUser(
        Instant timestamp, OrderId orderId, IPersistenceContext context, Seq<ReservationIndex> cancelledReservations, Amount fee, Transaction transaction) =>
        UpdateOrder(timestamp, orderId, cancelledReservations, UpdateUser(context, transaction), fee);

    static IPersistenceContext UpdateOrder(
        Instant timestamp, OrderId orderId, Seq<ReservationIndex> cancelledReservations, IPersistenceContext context, Amount fee) =>
        fee > Amount.Zero
            ? context.UpdateItem<Order>(Order.GetId(orderId), order => UpdateOrder(timestamp, cancelledReservations, fee, order))
            : context;

    static Order UpdateOrder(Instant timestamp, Seq<ReservationIndex> cancelledReservations, Amount fee, Order order) =>
        order with
        {
            User = order.User! with
            {
                AdditionalLineItems = order.User.AdditionalLineItems.Add(CreateLineItem(timestamp, cancelledReservations, fee))
            }
        };

    static LineItem CreateLineItem(Instant timestamp, Seq<ReservationIndex> cancelledReservations, Amount fee) =>
        new(timestamp, LineItemType.CancellationFee, new CancellationFee(cancelledReservations), null, -fee);

    static IPersistenceContext UpdateUser(IPersistenceContext context, Transaction transaction) =>
        context.UpdateItem<User>(user => UpdateUserBalance(user, transaction));

    static Amount GetFee(OrderingOptions options, bool waiveFee, Order order) =>
        waiveFee || order.NeedsConfirmation() ? Amount.Zero : options.CancellationFee;

    static IPersistenceContext AddTransaction(IPersistenceContext context, Transaction transaction) =>
        context.CreateItem(Transaction.GetId(transaction.TransactionId), transaction);

    static IPersistenceContext UpdateOrder(
        Instant timestamp, UserId updatedByUserId, HashSet<ReservationIndex> cancelledReservations, OrderId orderId, IPersistenceContext context) =>
        context.UpdateItem<Order>(Order.GetId(orderId), order => UpdateOrder(timestamp, updatedByUserId, cancelledReservations, context, order));

    static Order UpdateOrder(
        Instant timestamp, UserId updatedByUserId, HashSet<ReservationIndex> cancelledReservations, IPersistenceContext context, Order order) =>
        order with
        {
            Reservations = order.Reservations.Map((index, reservation) => UpdateReservation(cancelledReservations, reservation, index)).ToSeq(),
            Audits = order.Audits.Add(CreateAudit(timestamp, updatedByUserId, context))
        };

    static Reservation UpdateReservation(HashSet<ReservationIndex> cancelledReservations, Reservation reservation, int indexToUpdate) =>
        cancelledReservations.Find(index => index == indexToUpdate).Match(
            Some: _ => reservation with
            {
                Status = reservation.Status == ReservationStatus.Confirmed ? ReservationStatus.Cancelled : ReservationStatus.Abandoned,
                Cleaning = null
            },
            None: () => reservation);

    static OrderAudit CreateAudit(Instant timestamp, UserId userId, IPersistenceContext context) =>
        context.ItemOption<Transaction>().Case switch
        {
            Transaction transaction => new OrderAudit(timestamp, userId, OrderAuditType.CancelReservation, transaction.TransactionId),
            _ => new OrderAudit(timestamp, userId, OrderAuditType.CancelReservation)
        };

    public static (IPersistenceContext Context, bool IsCancellationWithoutFeeAllowed) TryAllowCancellationWithoutFee(
        OrderingOptions options, Instant timestamp, UserId userId, bool allowCancellationWithoutFee, Order order, IPersistenceContext context) =>
        (IsCancellationWithoutFeeEnabled(timestamp, order), allowCancellationWithoutFee) switch
        {
            (false, true) => (AllowCancellationWithoutFee(options, timestamp, userId, order.OrderId, context), true),
            (true, false) => (DisallowCancellationWithoutFee(timestamp, userId, order.OrderId, context), false),
            _ => (context, false)
        };

    static bool IsCancellationWithoutFeeEnabled(Instant timestamp, Order order) =>
        timestamp <= order.User!.NoFeeCancellationIsAllowedBefore;

    static IPersistenceContext AllowCancellationWithoutFee(OrderingOptions options, Instant timestamp, UserId userId, OrderId orderId, IPersistenceContext context) =>
        context.UpdateItem<Order>(Order.GetId(orderId), order => AllowCancellationWithoutFee(options, timestamp, userId, order));

    static Order AllowCancellationWithoutFee(OrderingOptions options, Instant timestamp, UserId userId, Order order) =>
        order with
        {
            User = order.User! with { NoFeeCancellationIsAllowedBefore = timestamp.Plus(options.CancellationWithoutFeeDuration) },
            Audits = order.Audits.Add(new OrderAudit(timestamp, userId, OrderAuditType.AllowCancellationWithoutFee))
        };

    static IPersistenceContext DisallowCancellationWithoutFee(Instant timestamp, UserId userId, OrderId orderId, IPersistenceContext context) =>
        context.UpdateItem<Order>(Order.GetId(orderId), order => DisallowCancellationWithoutFee(timestamp, userId, order));

    static Order DisallowCancellationWithoutFee(Instant timestamp, UserId userId, Order order) =>
        order with
        {
            User = order.User! with { NoFeeCancellationIsAllowedBefore = null },
            Audits = order.Audits.Add(new OrderAudit(timestamp, userId, OrderAuditType.DisallowCancellationWithoutFee))
        };

    public static Seq<Order> GetConfirmedOrders(Seq<Order> originalOrders, Seq<Order> updatedOrders) =>
        updatedOrders.Filter(order => order.IsConfirmed() && (!originalOrders.FirstOrDefault(o => o.OrderId == order.OrderId)?.IsConfirmed() ?? true));
}
