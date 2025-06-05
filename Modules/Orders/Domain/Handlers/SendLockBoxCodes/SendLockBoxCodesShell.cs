using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.LockBox.LockBoxCodesFunctions;
using static Frederikskaj2.Reservations.Orders.SendLockBoxCodes;
using static Frederikskaj2.Reservations.Persistence.QueryFactory;
using static Frederikskaj2.Reservations.Users.UsersFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

public static class SendLockBoxCodesShell
{
    static readonly IQuery<Order> ordersQuery =
        Query<Order>().Where(order => !order.Flags.HasFlag(OrderFlags.IsHistoryOrder) && !order.Flags.HasFlag(OrderFlags.IsOwnerOrder));

    public static EitherAsync<Failure<Unit>, Unit> SendLockBoxCodes(
        IOrdersEmailService emailService,
        OrderingOptions options,
        IEntityReader reader,
        IEntityWriter writer,
        SendLockBoxCodesCommand command,
        CancellationToken cancellationToken) =>
        from orderEntities in reader.QueryWithETag(ordersQuery, cancellationToken).MapReadError()
        from userExcerpts in ReadUserExcerpts(reader, toHashSet(orderEntities.Map(entity => entity.Value.UserId)), cancellationToken)
        from lockBoxCodes in ReadLockBoxCodes(reader, cancellationToken)
        let output = SendLockBoxCodesCore(options, new(command, orderEntities.ToValues(), userExcerpts, lockBoxCodes))
        from _1 in SendLockBoxCodesEmails(emailService, output.Emails, cancellationToken)
        from _2 in Write(writer, orderEntities, output.UpdatedOrders, cancellationToken)
        select unit;

    static EitherAsync<Failure<Unit>, Unit> SendLockBoxCodesEmails(
        IOrdersEmailService emailService, Seq<LockBoxCodesEmail> emails, CancellationToken cancellationToken) =>
        from _ in emails.Map(email => SendLockBoxCodesEmail(emailService, email, cancellationToken)).TraverseSerial(identity)
        select unit;

    static EitherAsync<Failure<Unit>, Unit> SendLockBoxCodesEmail(
        IOrdersEmailService emailService, LockBoxCodesEmail email, CancellationToken cancellationToken) =>
        emailService.Send(email, cancellationToken);

    static EitherAsync<Failure<Unit>, Seq<(EntityOperation Operation, ETag ETag)>> Write(
        IEntityWriter writer, Seq<ETaggedEntity<Order>> orderEntities, Seq<Order> updatedOrders, CancellationToken cancellationToken) =>
        writer
            .Write(
                collector => collector.Add(orderEntities),
                tracker => tracker.Update(updatedOrders),
                cancellationToken)
            .MapWriteError();
}
