using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using static Frederikskaj2.Reservations.Application.UserTransactionFactory;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

static class DatabaseFunctions
{
    const string partitionKey = "";

    public static EitherAsync<Failure, UserEmail> ReadUserEmail(IPersistenceContext context, EmailAddress email) =>
        MapReadError(context.Untracked.ReadItem<UserEmail>(EmailAddress.NormalizeEmail(email)));

    public static EitherAsync<Failure, User> ReadUser(IPersistenceContext context, UserId userId) =>
        MapReadError(context.Untracked.ReadItem<User>(User.GetId(userId)));

    public static EitherAsync<Failure, IEnumerable<User>> ReadUsers(IPersistenceContext context) =>
        MapReadError(context.Untracked.ReadItems(context.Query<User>().Where(user => !user.Flags.HasFlag(UserFlags.IsDeleted))));

    public static EitherAsync<Failure, IEnumerable<User>> ReadUsers(IPersistenceContext context, IEnumerable<UserId> userIds) =>
        userIds.Any()
            ? MapReadError(context.Untracked.ReadItems(context.Query<User>().Where(user => userIds.Contains(user.UserId))))
            : RightAsync<Failure, IEnumerable<User>>(Enumerable.Empty<User>());

    public static EitherAsync<Failure, IEnumerable<Order>> ReadOrders(IPersistenceContext context) =>
        MapReadError(context.Untracked.ReadItems(context.Query<Order>().Where(order => !order.Flags.HasFlag(OrderFlags.IsHistoryOrder))));

    public static EitherAsync<Failure, IEnumerable<Order>> ReadUserOrders(IPersistenceContext context) =>
        MapReadError(context.Untracked.ReadItems(
            context.Query<Order>().Where(order => !(order.Flags.HasFlag(OrderFlags.IsHistoryOrder) || order.Flags.HasFlag(OrderFlags.IsOwnerOrder)))));

    public static EitherAsync<Failure, IEnumerable<Transaction>> ReadTransactions(IPersistenceContext context, LocalDate fromDate, LocalDate toDate) =>
        MapReadError(context.Untracked.ReadItems(GetTransactionsQuery(context, fromDate, toDate)));

    static IProjectedQuery<Transaction> GetTransactionsQuery(IPersistenceContext context, LocalDate fromDate, LocalDate toDate) =>
        context.Query<Transaction>()
            .Where(transaction => fromDate <= transaction.Date && transaction.Date < toDate)
            .OrderBy(transaction => transaction.Date)
            .OrderBy(transaction => transaction.TransactionId)
            .Project();

    public static EitherAsync<Failure, IEnumerable<PostingV1>> ReadPostingsV1(IPersistenceContext context, LocalDate fromDate, LocalDate toDate) =>
        MapReadError(context.Untracked.ReadItems(GetPostingsV1Query(context, fromDate, toDate)));

    static IProjectedQuery<PostingV1> GetPostingsV1Query(IPersistenceContext context, LocalDate fromDate, LocalDate toDate) =>
        context.Query<PostingV1>()
            .Where(postingV1 => fromDate <= postingV1.Date && postingV1.Date < toDate)
            .OrderBy(posting => posting.Date)
            .OrderBy(posting => posting.TransactionId)
            .Project();

    public static EitherAsync<Failure, Option<Transaction>> ReadEarliestTransaction(IPersistenceContext context) =>
        from transactions in MapReadError(context.Untracked.ReadItems(context.Query<Transaction>().Top(1).OrderBy(transaction => transaction.Timestamp)))
        select transactions.Any() ? Some(transactions.First()) : None;

    public static EitherAsync<Failure, Option<Transaction>> ReadLatestTransaction(IPersistenceContext context) =>
        from transactions in MapReadError(context.Untracked.ReadItems(
            context.Query<Transaction>().Top(1).OrderByDescending(transaction => transaction.Timestamp)))
        select transactions.Any() ? Some(transactions.First()) : None;

    public static EitherAsync<Failure, IEnumerable<Transaction>> ReadTransactionsForUser(IPersistenceContext context, UserId userId) =>
        MapReadError(context.Untracked.ReadItems(
            context.Query<Transaction>()
                .Where(transaction => transaction.UserId == userId)
                .OrderBy(transaction => transaction.Date)
                .OrderBy(transaction => transaction.TransactionId)));

    public static EitherAsync<Failure, UserTransactions> ReadUserTransactions(IFormatter formatter, IPersistenceContext context, UserId userId) =>
        from transactions in ReadTransactionsForUser(context, userId)
        from user in ReadUserFullName(context, userId)
        select CreateUserTransactions(formatter, user, transactions);

    static EitherAsync<Failure, UserFullName> ReadUserFullName(IPersistenceContext context, UserId userId) =>
        from users in MapReadError(context.Untracked.ReadItems(GetUserQuery(context, userId)))
        let option = users.ToOption()
        from user in ItemOrNotFound(option)
        select user;

    static IProjectedQuery<UserFullName> GetUserQuery(IPersistenceContext context, UserId userId) =>
        context.Query<User>()
            .Where(user => user.UserId == userId)
            .ProjectTo(user => new UserFullName
            {
                UserId = user.UserId,
                FullName = user.FullName
            });

    static EitherAsync<Failure, T> ItemOrNotFound<T>(Option<T> option) =>
        option.Case switch
        {
            T item => item,
            _ => Failure.New(HttpStatusCode.NotFound)
        };

    public static EitherAsync<Failure, IEnumerable<UserFullName>> ReadUserFullNames(IPersistenceContext context, IEnumerable<UserId> userIds) =>
        userIds.Any()
            ? MapReadError(context.Untracked.ReadItems(GetUsersQuery(context, userIds)))
            : RightAsync<Failure, IEnumerable<UserFullName>>(Enumerable.Empty<UserFullName>());

    static IProjectedQuery<UserFullName> GetUsersQuery(IPersistenceContext context, IEnumerable<UserId> userIds) =>
        context.Query<User>()
            .Where(user => userIds.Contains(user.UserId))
            .ProjectTo(user => new UserFullName
            {
                UserId = user.UserId,
                FullName = user.FullName
            });

    public static EitherAsync<Failure, IPersistenceContext> ReadUserContext(IPersistenceContext context, UserId userId) =>
        MapReadError(context.ReadItem<User>(User.GetId(userId)));

    public static EitherAsync<Failure, IPersistenceContext> ReadUserAndOrdersIncludingHistoryOrdersContext(IPersistenceContext context, UserId userId) =>
        from context1 in ReadUserContext(context, userId)
        let user = context1.Item<User>()
        from context2 in ReadOrdersContext(context1, user.Orders.Concat(user.HistoryOrders))
        select context2;

    public static EitherAsync<Failure, IPersistenceContext> ReadUserAndAllOrdersContext(IPersistenceContext context, UserId userId) =>
        from context1 in ReadUserContext(context, userId)
        from context2 in MapReadError(context1.ReadItems(GetAllActiveOrdersQuery(context)))
        select context2;

    static IQuery<Order> GetAllActiveOrdersQuery(IPersistenceContext context) =>
        context.Query<Order>().Where(order => !order.Flags.HasFlag(OrderFlags.IsHistoryOrder));

    public static EitherAsync<Failure, IPersistenceContext> ReadUserAndOrdersContext(IPersistenceContext context, UserId userId) =>
        from context1 in ReadUserContext(context, userId)
        from context2 in MapReadError(context1.ReadItems(GetUserOrdersQuery(context1, userId)))
        select context2;

    static IQuery<Order> GetUserOrdersQuery(IPersistenceContext context, UserId userId) =>
        context.Query<Order>().Where(order => !order.Flags.HasFlag(OrderFlags.IsHistoryOrder) && order.UserId == userId);

    public static EitherAsync<Failure, IPersistenceContext> ReadOrderAndUserContext(IPersistenceContext context, OrderId orderId) =>
        from context1 in ReadOrderContext(context, orderId)
        let order = context1.Item<Order>()
        from context2 in ReadUserContext(context1, order.UserId)
        select context2;

    public static EitherAsync<Failure, IPersistenceContext> ReadAllOrdersAndUserFromOrderContext(IPersistenceContext context, OrderId orderId) =>
        from context1 in MapReadError(context.ReadItems(GetAllActiveOrdersQuery(context)))
        let order = context1.Order(orderId)
        from context2 in ReadUserContext(context1, order.UserId)
        select context2;

    public static EitherAsync<Failure, IPersistenceContext> ReadOrderContext(IPersistenceContext context, OrderId orderId) =>
        MapReadError(context.ReadItem<Order>(Order.GetId(orderId)));

    static EitherAsync<Failure, IPersistenceContext> ReadOrdersContext(IPersistenceContext context, IEnumerable<OrderId> orderIds) =>
        orderIds.Any()
            ? MapReadError(context.ReadItems(context.Query<Order>().Where(order => orderIds.Contains(order.OrderId))))
            : RightAsync<Failure, IPersistenceContext>(context);

    public static EitherAsync<Failure, IPersistenceContext> ReadUserOrdersContext(IPersistenceContext context) =>
        MapReadError(context.ReadItems(context.Query<Order>()
            .Where(order => !(order.Flags.HasFlag(OrderFlags.IsHistoryOrder) || order.Flags.HasFlag(OrderFlags.IsOwnerOrder)))));

    public static EitherAsync<Failure, IPersistenceContext> ReadOwnerOrdersContext(IPersistenceContext context) =>
        MapReadError(context.ReadItems(context.Query<Order>()
            .Where(order => !order.Flags.HasFlag(OrderFlags.IsHistoryOrder) && order.Flags.HasFlag(OrderFlags.IsOwnerOrder))));

    public static EitherAsync<Failure, IPersistenceContext> WriteContext(IPersistenceContext context) =>
        MapWriteError(context.Write());

    public static EitherAsync<Failure, T> MapReadError<T>(EitherAsync<HttpStatusCode, T> either) =>
        either.MapLeft(status => Failure.New(MapReadStatus(status), $"Database read error {status}."));

    public static EitherAsync<Failure, T> MapWriteError<T>(EitherAsync<HttpStatusCode, T> either) =>
        either.MapLeft(status => Failure.New(MapWriteStatus(status), $"Database write error {status}."));

    static HttpStatusCode MapReadStatus(HttpStatusCode status) =>
        status switch
        {
            HttpStatusCode.NotFound => status,
            _ => HttpStatusCode.ServiceUnavailable
        };

    public static HttpStatusCode MapReadStatusHideNotFound(HttpStatusCode status) =>
        status switch
        {
            HttpStatusCode.NotFound => HttpStatusCode.UnprocessableEntity,
            _ => HttpStatusCode.ServiceUnavailable
        };

    public static HttpStatusCode MapWriteStatus(HttpStatusCode status) =>
        status switch
        {
            HttpStatusCode.NotFound => status, // Replace or delete with wrong ID.
            HttpStatusCode.Conflict => status, // Create with existing ID or replace or delete with unmatched ETag.
            _ => HttpStatusCode.ServiceUnavailable
        };

    public static IPersistenceContext CreateContext(IPersistenceContextFactory contextFactory) =>
        contextFactory.Create(partitionKey);
}
