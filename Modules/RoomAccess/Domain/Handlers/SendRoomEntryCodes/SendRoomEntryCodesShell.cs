using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Persistence.QueryFactory;
using static Frederikskaj2.Reservations.RoomAccess.SendRoomEntryCodes;
using static Frederikskaj2.Reservations.Users.UsersFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.RoomAccess;

public static class SendRoomEntryCodesShell
{
    static readonly IQuery<Order> ordersQuery =
        Query<Order>().Where(order => !order.Flags.HasFlag(OrderFlags.IsHistoryOrder) && !order.Flags.HasFlag(OrderFlags.IsOwnerOrder));

    public static EitherAsync<Failure<Unit>, Unit> SendRoomEntryCodes(
        IRoomAccessEmailService emailService,
        OrderingOptions options,
        IEntityReader reader,
        IEntityWriter writer,
        SendRoomEntryCodesCommand command,
        CancellationToken cancellationToken) =>
        from orderEntities in reader.QueryWithETag(ordersQuery, cancellationToken).MapReadError()
        from userExcerpts in ReadUserExcerpts(reader, toHashSet(orderEntities.Map(entity => entity.Value.UserId)), cancellationToken)
        let output = SendRoomEntryCodesCore(options, new(command, orderEntities.ToValues(), userExcerpts))
        from _1 in SendRoomEntryCodeEmails(emailService, output.Emails, cancellationToken)
        from _2 in Write(writer, orderEntities, output.UpdatedOrders, cancellationToken)
        select unit;

    static EitherAsync<Failure<Unit>, Unit> SendRoomEntryCodeEmails(
        IRoomAccessEmailService emailService, Seq<RoomEntryCodeEmail> emails, CancellationToken cancellationToken) =>
        from _ in emails.Map(email => SendRoomEntryCodeEmail(emailService, email, cancellationToken)).TraverseSerial(identity)
        select unit;

    static EitherAsync<Failure<Unit>, Unit> SendRoomEntryCodeEmail(
        IRoomAccessEmailService emailService, RoomEntryCodeEmail email, CancellationToken cancellationToken) =>
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
