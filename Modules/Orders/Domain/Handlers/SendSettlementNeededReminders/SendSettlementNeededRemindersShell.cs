using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Orders.SendSettlementNeededReminders;
using static Frederikskaj2.Reservations.Persistence.QueryFactory;
using static Frederikskaj2.Reservations.Users.UsersFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

public static class SendSettlementNeededRemindersShell
{
    static readonly IQuery<Order> ordersQuery =
        Query<Order>().Where(order => !order.Flags.HasFlag(OrderFlags.IsHistoryOrder) && !order.Flags.HasFlag(OrderFlags.IsOwnerOrder));

    public static EitherAsync<Failure<Unit>, Unit> SendSettlementNeededReminders(
        IOrdersEmailService emailService,
        OrderingOptions options,
        IEntityReader reader,
        IEntityWriter writer,
        SendSettlementNeededRemindersCommand command,
        CancellationToken cancellationToken) =>
        from orderEntities in reader.QueryWithETag(ordersQuery, cancellationToken).MapReadError()
        from emailUsers in ReadSubscribedEmailUsers(reader, EmailSubscriptions.SettlementRequired, cancellationToken)
        let output = SendSettlementNeededRemindersCore(options, new(command, orderEntities.ToValues()))
        from _1 in SendSettlementNeededEmails(emailService, emailUsers, output.ReservationsToSettle, cancellationToken)
        from _2 in Write(writer, orderEntities, output.UpdatedOrders, cancellationToken)
        select unit;

    static EitherAsync<Failure<Unit>, Unit> SendSettlementNeededEmails(
        IOrdersEmailService emailService, Seq<EmailUser> users, Seq<ReservationWithOrder> reservations, CancellationToken cancellationToken) =>
        from _ in reservations.Map(reservation => SendSettlementNeededEmail(emailService, users, reservation, cancellationToken)).TraverseSerial(identity)
        select unit;

    static EitherAsync<Failure<Unit>, Unit> SendSettlementNeededEmail(
        IOrdersEmailService emailService, Seq<EmailUser> users, ReservationWithOrder reservation, CancellationToken cancellationToken) =>
        emailService.Send(
            new SettlementNeededEmailModel(reservation.Order.OrderId, reservation.Reservation.ResourceId, reservation.Reservation.Extent.Ends()),
            users,
            cancellationToken);

    static EitherAsync<Failure<Unit>, Seq<(EntityOperation Operation, ETag ETag)>> Write(
        IEntityWriter writer, Seq<ETaggedEntity<Order>> orderEntities, Seq<Order> updatedOrders, CancellationToken cancellationToken) =>
        writer
            .Write(
                collector => collector.Add(orderEntities),
                tracker => tracker.Update(updatedOrders),
                cancellationToken)
            .MapWriteError();
}
