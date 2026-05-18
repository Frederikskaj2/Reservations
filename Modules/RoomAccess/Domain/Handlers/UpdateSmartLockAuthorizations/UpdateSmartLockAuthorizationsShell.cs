using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using NodaTime;
using System.Threading;
using static Frederikskaj2.Reservations.Persistence.QueryFactory;
using static Frederikskaj2.Reservations.RoomAccess.UpdateSmartLockAuthorizations;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.RoomAccess;

public static class UpdateSmartLockAuthorizationsShell
{
    public static EitherAsync<Failure<Unit>, Unit> UpdateSmartLockAuthorizations(
        OrderingOptions options,
        IEntityReader reader,
        ISmartLockService smartLockService,
        ITimeConverter timeConverter,
        UpdateSmartLockAuthorizationsCommand command,
        CancellationToken cancellationToken) =>
        from context in GetContext(smartLockService, cancellationToken)
        from reservations in ReadReservations(reader, command.Date.Plus(options.RevealEntryCodeBeforeReservationStart), cancellationToken)
        let output = UpdateSmartLockAuthorizationsCore(options, timeConverter, new(command, context, reservations))
        from _ in SynchronizeContext(smartLockService, output.SmartLockAuthorizationContext, cancellationToken)
        select unit;

    static EitherAsync<Failure<Unit>, ISmartLockAuthorizationContext> GetContext(ISmartLockService smartLockService, CancellationToken cancellationToken) =>
        smartLockService.GetSmartLockAuthorizationContext(cancellationToken);

    static EitherAsync<Failure<Unit>, Seq<ReservationInformation>> ReadReservations(
        IEntityReader entityReader, LocalDate startNoLaterThan, CancellationToken cancellationToken) =>
        entityReader.Query(GetQuery(startNoLaterThan), cancellationToken).MapReadError();

    static IQuery<ReservationInformation> GetQuery(LocalDate startNoLaterThan) =>
        Query<Order>()
            .Where(order => !order.Flags.HasFlag(OrderFlags.IsHistoryOrder))
            .Join(
                order => order.Reservations,
                reservation => reservation.EntryCode != null && reservation.Extent.Date <= startNoLaterThan,
                (order, reservation) => new ReservationInformation(order.OrderId, reservation.ResourceId, reservation.Extent, reservation.EntryCode!.Value));

    static EitherAsync<Failure<Unit>, Unit> SynchronizeContext(
        ISmartLockService smartLockService, ISmartLockAuthorizationContext context, CancellationToken cancellationToken) =>
        smartLockService.SynchronizeSmartLockAuthorizationSet(context, cancellationToken);
}
