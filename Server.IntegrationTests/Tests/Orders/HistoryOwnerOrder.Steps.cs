using FluentAssertions;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Framework;
using LightBDD.XUnit2;
using NodaTime;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

sealed partial class HistoryOwnerOrder(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    const int additionalDaysWhereCleaningCanBeDone = 3;

    State<OrderDetailsDto> order;
    State<OrderDetailsDto> order2;

    OrderDetailsDto Order => order.GetValue(nameof(Order));
    OrderDetailsDto Order2 => order2.GetValue(nameof(Order2));

    Task IScenarioSetUp.OnScenarioSetUp()
    {
        session.NowOffset = Period.Zero;
        return Task.CompletedTask;
    }

    async Task GivenAnOwnerOrder()
    {
        var placeOwnerOrderResponse = await session.PlaceOwnerOrder(new TestReservation(SeedData.BanquetFacilities.ResourceId, 2));
        order = placeOwnerOrderResponse.Order;
    }

    Task GivenTheOrderIsInThePast()
    {
        session.NowOffset += GetDaysUntilFinish(Order.Reservations.Single());
        return Task.CompletedTask;
    }

    async Task GivenSecondOwnerOrderAtALaterTime()
    {
        var placeOwnerOrderResponse = await session.PlaceOwnerOrder(new TestReservation(SeedData.BanquetFacilities.ResourceId, 2, 7));
        order2 = placeOwnerOrderResponse.Order;
    }

    async Task GivenAnOwnerOrderWithTwoReservations()
    {
        var orderDetails = await session.PlaceOwnerOrder(
            new TestReservation(SeedData.BanquetFacilities.ResourceId, 2),
            new TestReservation(SeedData.BanquetFacilities.ResourceId, 2, 14));
        order = orderDetails.Order;
    }

    Task GivenTheFirstReservationIsInThePast()
    {
        session.NowOffset += GetDaysUntilFinish(Order.Reservations.First());
        return Task.CompletedTask;
    }

    Task GivenBothReservationsAreInThePast()
    {
        session.NowOffset += GetDaysUntilFinish(Order.Reservations.Last());
        return Task.CompletedTask;
    }

    async Task WhenTheJobToFinishOwnerOrdersHasExecuted() => await session.FinishOwnerOrders();

    async Task ThenTheOrderBecomesAHistoryOrder()
    {
        var orderResponse = await session.GetOrder(Order.OrderId);
        orderResponse.Order.IsHistoryOrder.Should().BeTrue();
        orderResponse.Order.Audits.Last().Type.Should().Be(OrderAuditType.FinishOrder);
        orderResponse.Order.Audits.Last().Timestamp.Should().Be(
            Order.Reservations.Last().Extent.Ends().AtMidnight().InZoneLeniently(session.TimeZone).ToInstant());
    }

    async Task ThenTheSecondOrderIsNotAHistoryOrder()
    {
        var orderResponse = await session.GetOrder(Order2.OrderId);
        orderResponse.Order.IsHistoryOrder.Should().BeFalse();
    }

    async Task ThenTheOrderIsNotAHistoryOrder()
    {
        var orderResponse = await session.GetOrder(Order.OrderId);
        orderResponse.Order.IsHistoryOrder.Should().BeFalse();
    }

    Period GetDaysUntilFinish(ReservationDto reservation) =>
        Period.FromDays(Period.Between(session.CurrentDate, reservation.Extent.Ends(), PeriodUnits.Days).Days + 1 + additionalDaysWhereCleaningCanBeDone);
}
