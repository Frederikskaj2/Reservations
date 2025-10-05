using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;
using System.Threading;
using static Frederikskaj2.Reservations.Orders.ConfirmOrders;
using static Frederikskaj2.Reservations.Persistence.QueryFactory;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

public static class ConfirmOrdersShell
{
    static readonly IQuery<Order> ordersQuery =
        Query<Order>().Where(order => !order.Flags.HasFlag(OrderFlags.IsHistoryOrder) && !order.Flags.HasFlag(OrderFlags.IsOwnerOrder));

    public static EitherAsync<Failure<Unit>, Unit> ConfirmOrders(
        IOrdersEmailService emailService,
        IJobScheduler jobScheduler,
        IEntityReader reader,
        IEntityWriter writer,
        ConfirmOrdersCommand command,
        CancellationToken cancellationToken) =>
        from orderEntities in reader.QueryWithETag(ordersQuery, cancellationToken).MapReadError()
        let unconfirmedOrderEntities =
            orderEntities.Filter(entity => entity.Value.Reservations.Any(reservation => reservation.Status is ReservationStatus.Reserved))
        from userEntities in ReadUserEntities(reader, unconfirmedOrderEntities, cancellationToken)
        from transactions in ReadTransactions(reader, unconfirmedOrderEntities, cancellationToken)
        let output = ConfirmOrdersCore(new(command, userEntities.ToValues(), unconfirmedOrderEntities.ToValues(), transactions))
        from _1 in Write(writer, userEntities, unconfirmedOrderEntities, output, cancellationToken)
        from _2 in SendOrderConfirmedEmails(emailService, output.UsersWithOrders, cancellationToken)
        let areOrdersConfirmed = !output.UsersWithOrders.IsEmpty
        from _3 in UpdateCleaningSchedule(jobScheduler, areOrdersConfirmed).ToRightAsync<Failure<Unit>, Unit>()
        select unit;

    static EitherAsync<Failure<Unit>, Seq<ETaggedEntity<User>>> ReadUserEntities(
        IEntityReader reader, Seq<ETaggedEntity<Order>> orderEntities, CancellationToken cancellationToken) =>
        ReadUserEntities(reader, toHashSet(orderEntities.Map(entity => entity.Value.UserId)), cancellationToken);

    static EitherAsync<Failure<Unit>, Seq<ETaggedEntity<User>>> ReadUserEntities(
        IEntityReader reader, HashSet<UserId> residentIds, CancellationToken cancellationToken) =>
        !residentIds.IsEmpty
            ? reader.QueryWithETag(Query<User>().Where(user => residentIds.Contains(user.UserId)), cancellationToken).MapReadError()
            : Seq<ETaggedEntity<User>>();

    static EitherAsync<Failure<Unit>, Seq<TransactionExcerpt>> ReadTransactions(
        IEntityReader reader, Seq<ETaggedEntity<Order>> orderEntities, CancellationToken cancellationToken) =>
        !orderEntities.IsEmpty
            ? ReadTransactions(reader, GetTransactionsQueryParameters(orderEntities), cancellationToken)
            : Seq<TransactionExcerpt>();

    static (HashSet<UserId> ResidentIds, Instant OnOrAfter) GetTransactionsQueryParameters(Seq<ETaggedEntity<Order>> orderEntities) =>
        orderEntities
            .ToValues()
            .Fold(
                (ResidentIds: LanguageExt.HashSet<UserId>.Empty, OnOrAfter: Instant.MaxValue),
                (tuple, order) => (tuple.ResidentIds.TryAdd(order.UserId), order.CreatedTimestamp < tuple.OnOrAfter ? order.CreatedTimestamp : tuple.OnOrAfter));

    static EitherAsync<Failure<Unit>, Seq<TransactionExcerpt>> ReadTransactions(
        IEntityReader reader, (HashSet<UserId> ResidentIds, Instant OnOrAfter) tuple, CancellationToken cancellationToken) =>
        ReadTransactions(reader, tuple.ResidentIds, tuple.OnOrAfter, cancellationToken);

    static EitherAsync<Failure<Unit>, Seq<TransactionExcerpt>> ReadTransactions(
        IEntityReader reader, HashSet<UserId> residentIds, Instant onOrAfter, CancellationToken cancellationToken) =>
        reader
            .Query(
                Query<Transaction>()
                    .Where(transaction => residentIds.Contains(transaction.ResidentId) && transaction.Timestamp >= onOrAfter)
                    .Project(transaction =>
                        new TransactionExcerpt(transaction.TransactionId, transaction.AdministratorId, transaction.Timestamp, transaction.ResidentId)),
                cancellationToken)
            .MapReadError();

    static EitherAsync<Failure<Unit>, Seq<(EntityOperation Operation, ETag ETag)>> Write(
        IEntityWriter writer,
        Seq<ETaggedEntity<User>> userEntities,
        Seq<ETaggedEntity<Order>> unconfirmedOrderEntities,
        ConfirmOrdersOutput output,
        CancellationToken cancellationToken) =>
        writer
            .Write(
                collector => collector.Add(userEntities).Add(unconfirmedOrderEntities),
                tracker => tracker
                    .Update(output.UsersWithOrders.Map(userWithOrders => userWithOrders.User))
                    .Update(output.UsersWithOrders.Bind(uwo => uwo.Orders)),
                cancellationToken)
            .MapWriteError();

    static EitherAsync<Failure<Unit>, Unit> SendOrderConfirmedEmails(
        IOrdersEmailService emailService, Seq<UserWithOrders> usersWithOrders, CancellationToken cancellationToken) =>
        from _ in usersWithOrders.Map(userWithOrder => SendOrderConfirmedEmails(emailService, userWithOrder, cancellationToken)).TraverseSerial(identity)
        select unit;

    static EitherAsync<Failure<Unit>, Unit> SendOrderConfirmedEmails(
        IOrdersEmailService emailService, UserWithOrders userWithOrders, CancellationToken cancellationToken) =>
        from _ in userWithOrders.Orders
            .Map(order => SendOrderConfirmedEmail(emailService, userWithOrders.User, order, cancellationToken))
            .TraverseSerial(identity)
        select unit;

    static EitherAsync<Failure<Unit>, Unit> SendOrderConfirmedEmail(
        IOrdersEmailService emailService, User user, Order order, CancellationToken cancellationToken) =>
        emailService.Send(new OrderConfirmedEmailModel(user.Email(), user.FullName, order.OrderId), cancellationToken);

    static Unit UpdateCleaningSchedule(IJobScheduler jobScheduler, bool areOrdersConfirmed) =>
        areOrdersConfirmed ? jobScheduler.Queue(JobName.UpdateCleaningSchedule) : unit;
}
