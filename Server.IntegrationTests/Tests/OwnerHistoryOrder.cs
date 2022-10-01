using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using NodaTime;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests;

public class OwnerHistoryOrder : IClassFixture<SessionFixture>
{
    public OwnerHistoryOrder(SessionFixture session) => Session = session;

    SessionFixture Session { get; }


    [Fact]
    public async Task OwnerOrderBecomesHistoryOrder()
    {
        var ownerOrder = await Session.PlaceOwnerOrderAsync(new TestReservation(TestData.BanquetFacilities.ResourceId, 2));
        var days = (ownerOrder.Reservations!.Single().Extent.Ends() - Session.CurrentDate).Days + 1;
        Session.NowOffset += Period.FromDays(days);
        var order = await Session.GetOrderAsync(ownerOrder.OrderId);
        order.IsHistoryOrder.Should().BeTrue();
    }

    [Fact]
    public async Task OwnerOrderDoesNotBecomeHistoryOrderWhenFirstReservationIsInThePast()
    {
        var ownerOrder = await Session.PlaceOwnerOrderAsync(
            new TestReservation(TestData.BanquetFacilities.ResourceId, 2),
            new TestReservation(TestData.BanquetFacilities.ResourceId, 2));
        var days = (ownerOrder.Reservations!.First().Extent.Ends() - Session.CurrentDate).Days + 1;
        Session.NowOffset += Period.FromDays(days);
        var order = await Session.GetOrderAsync(ownerOrder.OrderId);
        order.IsHistoryOrder.Should().BeFalse();
    }

    [Fact]
    public async Task OwnerOrderBecomesHistoryOrderWhenSecondReservationIsInThePast()
    {
        var ownerOrder = await Session.PlaceOwnerOrderAsync(
            new TestReservation(TestData.BanquetFacilities.ResourceId, 2),
            new TestReservation(TestData.BanquetFacilities.ResourceId, 2));
        var days = (ownerOrder.Reservations!.Last().Extent.Ends() - Session.CurrentDate).Days + 1;
        Session.NowOffset += Period.FromDays(days);
        var order = await Session.GetOrderAsync(ownerOrder.OrderId);
        order.IsHistoryOrder.Should().BeTrue();
    }
}