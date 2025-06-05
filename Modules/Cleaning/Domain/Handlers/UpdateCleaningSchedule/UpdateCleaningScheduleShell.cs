using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using NodaTime;
using System.Threading;
using static Frederikskaj2.Reservations.Cleaning.UpdateCleaningSchedule;
using static Frederikskaj2.Reservations.Orders.OrdersQueries;
using static Frederikskaj2.Reservations.Persistence.QueryFactory;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Cleaning;

public static class UpdateCleaningScheduleShell
{
    public static EitherAsync<Failure<Unit>, Unit> UpdateCleaningSchedule(
        OrderingOptions options,
        IEntityReader reader,
        IEntityWriter writer,
        UpdateCleaningScheduleCommand command,
        CancellationToken cancellationToken) =>
        from activeOrders in reader.Query(GetAllActiveOrdersQuery.Project(), cancellationToken).MapReadError()
        from recentOrders in reader.Query(GetRecentOrdersQuery(options, command), cancellationToken).MapReadError()
        let allOrders = activeOrders.Concat(recentOrders).Distinct(order => order.OrderId)
        let output = UpdateCleaningScheduleCore(options, new(command, allOrders))
        from _ in writer.Write(tracker => tracker.Upsert(output.CleaningSchedule), cancellationToken).MapWriteError()
        select unit;

    static IProjectedQuery<Order> GetRecentOrdersQuery(OrderingOptions options, UpdateCleaningScheduleCommand command) =>
        GetRecentOrdersQuery(command.StartDate.Minus(options.RecentOrdersMaximumAge), command.StartDate);

    static IProjectedQuery<Order> GetRecentOrdersQuery(LocalDate onOrAfter, LocalDate before) =>
        Query<Order>()
            .Where(order => order.Flags.HasFlag(OrderFlags.IsCleaningRequired))
            .Join(
                order => order.Reservations,
                reservation => onOrAfter <= reservation.Extent.Date && reservation.Extent.Date < before,
                (order, _) => order)
            .Distinct()
            .Project();
}
