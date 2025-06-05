using FluentAssertions;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using NodaTime;
using Xunit;
using Xunit.Abstractions;

namespace Frederikskaj2.Reservations.Persistence.UnitTests;

public class CosmosSqlTests(ITestOutputHelper output)
{
    [Fact]
    public void Query()
    {
        var query = QueryFactory.Query<Order>();
        var sql = query.Sql;
        output.WriteLine(sql);
        sql.Should()
            .Be(
                """
                select value root from root where root.d = "Order"
                """);
   }

    // Test IsMap code path.

    [Fact]
    public void Project()
    {
        var query = QueryFactory.Query<Order>().Project();
        var sql = query.Sql;
        output.WriteLine(sql);
        sql.Should()
            .Be(
                """
                select value root.v from root where root.d = "Order"
                """);
    }

    [Fact]
    public void ProjectArrayElement()
    {
        var query = QueryFactory.Query<User>().Project(user => new { user.Emails[0].Email });
        var sql = query.Sql;
        output.WriteLine(sql);
        sql.Should()
            .Be(
                """
                select root.v.emails[0].email as email from root where root.d = "User"
                """);
    }

    [Fact]
    public void FilterOneOfAndProjectToClass()
    {
        var query = QueryFactory.Query<Order>()
            .Where(order => order.Specifics.IsT0)
            .Project(order => new ResidentOrderClass { OrderId = order.OrderId, Order = order.Specifics.AsT0 });
        var sql = query.Sql;
        output.WriteLine(sql);
        sql.Should()
            .Be(
                """
                select root.v.orderId as orderId, root.v.specifics.resident as order from root where root.d = "Order" and is_defined(root.v.specifics.resident)
                """);
    }

    [Fact]
    public void FilterOneOfAndProjectToAnonymousType()
    {
        var query = QueryFactory.Query<Order>()
            .Where(order => order.Specifics.IsT0)
            .Project(order => new { order.OrderId, Order = order.Specifics.AsT0 });
        var sql = query.Sql;
        output.WriteLine(sql);
        sql.Should()
            .Be(
                """
                select root.v.orderId as orderId, root.v.specifics.resident as order from root where root.d = "Order" and is_defined(root.v.specifics.resident)
                """);
    }

    [Fact]
    public void FilterOneOfAndProjectToRecord()
    {
        var query = QueryFactory.Query<Order>()
            .Where(order => order.Specifics.IsT0)
            .Project(order => new ResidentOrderRecord(order.OrderId, order.Specifics.AsT0));
        var sql = query.Sql;
        output.WriteLine(sql);
        sql.Should()
            .Be(
                """
                select root.v.orderId as orderId, root.v.specifics.resident as order from root where root.d = "Order" and is_defined(root.v.specifics.resident)
                """);
    }

    [Fact]
    public void FilterOneOfAndProjectToScalar()
    {
        var query = QueryFactory.Query<Order>()
            .Where(order => order.Specifics.IsT0)
            .Project(order => order.Specifics.AsT0.NoFeeCancellationIsAllowedBefore);
        var sql = query.Sql;
        output.WriteLine(sql);
        sql.Should()
            .Be(
                """
                select value root.v.specifics.resident.noFeeCancellationIsAllowedBefore from root where root.d = "Order" and is_defined(root.v.specifics.resident)
                """);
    }

    [Fact]
    public void JoinFiltered()
    {
        var query = QueryFactory.Query<Order>()
            .Where(order => !order.Flags.HasFlag(OrderFlags.IsOwnerOrder))
            .Join(
                order => order.Reservations,
                reservation => reservation.Status == ReservationStatus.Reserved || reservation.Status == ReservationStatus.Confirmed);
        var sql = query.Sql;
        output.WriteLine(sql);
        sql.Should()
            .Be(
                """
                select value child from root join child in root.v.reservations where root.d = "Order" and not ((is_defined(root.v.flags) and root.v.flags & 4 = 4)) and (((child.status) = 1) or ((child.status) = 3))
                """);
    }

    [Fact]
    public void JoinFilteredAndProjected()
    {
        var query = QueryFactory.Query<Order>()
            .Where(order => !order.Flags.HasFlag(OrderFlags.IsOwnerOrder))
            .Join(
                order => order.Reservations,
                reservation => reservation.Status == ReservationStatus.Reserved || reservation.Status == ReservationStatus.Confirmed,
                (order, reservation) => new
                {
                    order.OrderId,
                    order.UserId,
                    reservation.ResourceId,
                    reservation.Extent,
                });
        var sql = query.Sql;
        output.WriteLine(sql);
        sql.Should()
            .Be(
                """
                select root.v.orderId as orderId, root.v.userId as userId, child.resourceId as resourceId, child.extent as extent from root join child in root.v.reservations where root.d = "Order" and not ((is_defined(root.v.flags) and root.v.flags & 4 = 4)) and (((child.status) = 1) or ((child.status) = 3))
                """);
    }

    [Fact]
    public void JoinToBeAbleToFilterOnChild()
    {
        var onOrAfter = new LocalDate(2025, 6, 4);
        var query = QueryFactory.Query<Order>()
            .Where(order => order.Flags.HasFlag(OrderFlags.IsCleaningRequired))
            .Join(
                order => order.Reservations,
                reservation => reservation.Extent.Date >= onOrAfter,
                (order, _) => order)
            .Project();

        var sql = query.Sql;
        output.WriteLine(sql);
        sql.Should()
            .Be(
                """
                select value root.v from root join child in root.v.reservations where root.d = "Order" and (is_defined(root.v.flags) and root.v.flags & 1 = 1) and (child.extent.date >= "2025-06-04")
                """);
    }
}
