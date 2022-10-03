using System.Linq;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

static class OwnerOrderFunctions
{
    public static EitherAsync<Failure, IPersistenceContext> MakeHistoryOwnerOrder(
        IDateProvider dateProvider, OrderingOptions options, LocalDate today, IPersistenceContext context) =>
        MakeHistoryOwnerOrder(dateProvider, options, today, context, context.Item<Order>());

    static EitherAsync<Failure, IPersistenceContext> MakeHistoryOwnerOrder(
        IDateProvider dateProvider, OrderingOptions options, LocalDate today, IPersistenceContext context, Order order) =>
        order.Flags.HasFlag(OrderFlags.IsOwnerOrder)
            ? TryMakeHistoryOwnerOrder(dateProvider, options, today, context, order)
            : RightAsync<Failure, IPersistenceContext>(context);

    static EitherAsync<Failure, IPersistenceContext> TryMakeHistoryOwnerOrder(
        IDateProvider dateProvider, OrderingOptions options, LocalDate today, IPersistenceContext context, Order order) =>
        WriteContext(TryMakeImplicitHistoryOrder(dateProvider, options, today, context, order));

    public static IPersistenceContext TryMakeHistoryOwnerOrders(
        IDateProvider dateProvider, OrderingOptions options, Instant timestamp, LocalDate today, Option<OrderId> orderWithCancellations, IPersistenceContext context) =>
        TryMakeImplicitHistoryOrders(
            dateProvider,
            options,
            today,
            TryMakeExplicitHistoryOrder(options, timestamp, today, orderWithCancellations, context));

    static IPersistenceContext TryMakeExplicitHistoryOrder(
        OrderingOptions options, Instant timestamp, LocalDate today, Option<OrderId> orderWithCancellations, IPersistenceContext context) =>
        orderWithCancellations.Case switch
        {
            OrderId orderId => TryMakeExplicitHistoryOrder(options, timestamp, today, context, context.Order(orderId)),
            _ => context
        };

    static IPersistenceContext TryMakeImplicitHistoryOrders(
        IDateProvider dateProvider, OrderingOptions options, LocalDate today, IPersistenceContext context) =>
        TryMakeImplicitHistoryOrders(dateProvider, options, today, context, context.Items<Order>().ToSeq());

    static IPersistenceContext TryMakeImplicitHistoryOrders(IDateProvider dateProvider, OrderingOptions options, LocalDate today, IPersistenceContext context, Seq<Order> orders) =>
        orders.Match(
            () => context,
            (head, tail) => TryMakeImplicitHistoryOrders(dateProvider, options, today, context, head, tail));

    static IPersistenceContext TryMakeImplicitHistoryOrders(
        IDateProvider dateProvider, OrderingOptions options, LocalDate today, IPersistenceContext context, Order order, Seq<Order> orders) =>
        TryMakeImplicitHistoryOrders(dateProvider, options, today, TryMakeImplicitHistoryOrder(dateProvider, options, today, context, order), orders);

    static IPersistenceContext TryMakeImplicitHistoryOrder(
        IDateProvider dateProvider, OrderingOptions options, LocalDate today, IPersistenceContext context, Order order) =>
        ShouldBeHistoryOrder(options, today, order) ? MakeImplicitHistoryOrder(dateProvider, context, order) : context;

    public static EitherAsync<Failure, IPersistenceContext> MakeHistoryOwnerOrders(
        IDateProvider dateProvider, OrderingOptions options, LocalDate today, IPersistenceContext context) =>
        WriteContext(TryMakeHistoryOrders(dateProvider, options, today, context, context.Items<Order>().ToSeq()));

    static IPersistenceContext TryMakeHistoryOrders(
        IDateProvider dateProvider, OrderingOptions options, LocalDate today, IPersistenceContext context, Seq<Order> orders) =>
        orders.Match(
            () => context,
            (head, tail) => TryMakeHistoryOrders(dateProvider, options, today, context, head, tail));

    static IPersistenceContext TryMakeHistoryOrders(
        IDateProvider dateProvider, OrderingOptions options, LocalDate today, IPersistenceContext context, Order order, Seq<Order> orders) =>
        TryMakeHistoryOrders(dateProvider, options, today, TryMakeHistoryOrder(dateProvider, options, today, context, order), orders);

    static IPersistenceContext TryMakeHistoryOrder(
        IDateProvider dateProvider, OrderingOptions options, LocalDate today, IPersistenceContext context, Order order) =>
        ShouldBeHistoryOrder(options, today, order)
            ? MakeImplicitHistoryOrder(GetLatestReservationEndTimestamp(dateProvider, order), context, order.OrderId)
            : context;

    static bool ShouldBeHistoryOrder(OrderingOptions options, LocalDate today, Order order) =>
        order.Flags.HasFlag(OrderFlags.IsOwnerOrder) && !order.Flags.HasFlag(OrderFlags.IsHistoryOrder) &&
        order.Reservations.All(reservation =>
            reservation.Status is ReservationStatus.Cancelled || reservation.Extent.Ends().PlusDays(options.AdditionalDaysWhereCleaningCanBeDone) < today);

    static Instant GetLatestReservationEndTimestamp(IDateProvider dateProvider, Order order) =>
        dateProvider.GetMidnight(
            order.Reservations
                .Filter(reservation => reservation.Status == ReservationStatus.Confirmed)
                .OrderByDescending(reservation => reservation.Extent.Ends())
                .First().Extent.Ends());

    static IPersistenceContext TryMakeExplicitHistoryOrder(
        OrderingOptions options, Instant timestamp, LocalDate today, IPersistenceContext context, Order order) =>
        ShouldBeHistoryOrder(options, today, order) ? MakeExplicitHistoryOrder(timestamp, context, order.OrderId) : context;

    static IPersistenceContext MakeExplicitHistoryOrder(Instant timestamp, IPersistenceContext context, OrderId orderId) =>
        context.UpdateItem<Order>(Order.GetId(orderId), order => MakeExplicitHistoryOrder(timestamp, order));

    static Order MakeExplicitHistoryOrder(Instant timestamp, Order order) =>
        order with
        {
            Flags = order.Flags | OrderFlags.IsHistoryOrder,
            Audits = order.Audits.Add(new OrderAudit(timestamp, null, OrderAuditType.FinishOrder))
        };

    static IPersistenceContext MakeImplicitHistoryOrder(IDateProvider dateProvider, IPersistenceContext context, Order order) =>
        MakeImplicitHistoryOrder(GetLatestReservationEndTimestamp(dateProvider, order), context, order.OrderId);

    static IPersistenceContext MakeImplicitHistoryOrder(Instant timestamp, IPersistenceContext context, OrderId orderId) =>
        context.UpdateItem<Order>(Order.GetId(orderId), order => MakeImplicitHistoryOrder(timestamp, order));

    static Order MakeImplicitHistoryOrder(Instant timestamp, Order order) =>
        order with
        {
            Flags = order.Flags | OrderFlags.IsHistoryOrder,
            Audits = order.Audits.Add(new OrderAudit(timestamp, null, OrderAuditType.FinishOrder))
        };

    public static EitherAsync<Failure, IPersistenceContext> UpdateOwnerOrder(UpdateOwnerOrderCommand command, IPersistenceContext context) =>
        RightAsync<Failure, IPersistenceContext>(TryUpdateCleaning(command, TryUpdateDescription(command, context)));

    static IPersistenceContext TryUpdateDescription(UpdateOwnerOrderCommand command, IPersistenceContext context) =>
        command.Description.Case switch
        {
            string description => UpdateDescription(command.Timestamp, command.UserId, command.OrderId, description, context),
            _ => context
        };

    static IPersistenceContext UpdateDescription(Instant timestamp, UserId userId, OrderId orderId, string description, IPersistenceContext context) =>
        context.UpdateItem<Order>(Order.GetId(orderId), order => TryUpdateDescription(timestamp, userId, description, order));

    static Order TryUpdateDescription(Instant timestamp, UserId userId, string description, Order order) =>
        description != order.Owner!.Description ? UpdateDescription(timestamp, userId, description, order) : order;

    static Order UpdateDescription(Instant timestamp, UserId userId, string description, Order order) =>
        order with
        {
            Owner = new(description),
            Audits = order.Audits.Add(new OrderAudit(timestamp, userId, OrderAuditType.UpdateDescription))
        };

    static IPersistenceContext TryUpdateCleaning(UpdateOwnerOrderCommand command, IPersistenceContext context) =>
        command.IsCleaningRequired.Case switch
        {
            bool isCleaningRequired => UpdateCleaning(command.Timestamp, command.UserId, command.OrderId, isCleaningRequired, context),
            _ => context
        };

    static IPersistenceContext UpdateCleaning(Instant timestamp, UserId userId, OrderId orderId, bool isCleaningRequired, IPersistenceContext context) =>
        context.UpdateItem<Order>(Order.GetId(orderId), order => TryUpdateCleaning(timestamp, userId, isCleaningRequired, order));

    static Order TryUpdateCleaning(Instant timestamp, UserId userId, bool isCleaningRequired, Order order) =>
        isCleaningRequired && !order.Flags.HasFlag(OrderFlags.IsCleaningRequired) ||
        !isCleaningRequired && order.Flags.HasFlag(OrderFlags.IsCleaningRequired)
            ? UpdateCleaning(timestamp, userId, isCleaningRequired, order)
            : order;

    static Order UpdateCleaning(Instant timestamp, UserId userId, bool isCleaningRequired, Order order) =>
        order with
        {
            Flags = GetFlags(order.Flags, isCleaningRequired),
            Audits = order.Audits.Add(new OrderAudit(timestamp, userId, OrderAuditType.UpdateCleaning))
        };

    static OrderFlags GetFlags(OrderFlags flags, bool isCleaningRequired) =>
        isCleaningRequired ? flags | OrderFlags.IsCleaningRequired : flags & ~OrderFlags.IsCleaningRequired;
}
