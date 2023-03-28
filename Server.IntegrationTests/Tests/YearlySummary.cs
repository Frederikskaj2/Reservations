using FluentAssertions;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Shared.Core;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests;

public class YearlySummary : IClassFixture<SessionFixture>
{
    public YearlySummary(SessionFixture session) => Session = session;

    SessionFixture Session { get; }

    [Fact]
    public async Task GetYearlySummaryRangeWithNoOrders()
    {
        var yearlySummaryRange = await Session.GetYearlySummaryRange();
        yearlySummaryRange.EarliestYear.Should().Be(yearlySummaryRange.LatestYear);
    }

    [Fact]
    public async Task GetYearlySummaryRangeWithAnOrder()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var yearlySummaryRange = await Session.GetYearlySummaryRange();
        var year = userOrder.Reservations!.First().Extent.Date.Year;
        yearlySummaryRange.EarliestYear.Should().Be(year);
        yearlySummaryRange.LatestYear.Should().Be(year);
    }

    [Fact]
    public async Task GetYearlySummaryWithNoOrders()
    {
        var yearlySummaryRange = await Session.GetYearlySummaryRange();
        var yearlySummary = await Session.GetYearlySummary(yearlySummaryRange.EarliestYear);
        yearlySummary.Year.Should().Be(yearlySummaryRange.EarliestYear);
        yearlySummary.ResourceSummaries.Should().BeEmpty();
    }

    [Fact]
    public async Task GetYearlySummaryWithThreeOrders()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder1 = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var userOrder2 = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var userOrder3 = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.BanquetFacilities.ResourceId));
        var yearlySummaryRange = await Session.GetYearlySummaryRange();
        var year = yearlySummaryRange.LatestYear;
        var yearlySummary = await Session.GetYearlySummary(year);
        yearlySummary.Year.Should().Be(year);
        var bedroomSummary = userOrder1.Reservations!.Concat(userOrder2.Reservations!).Where(reservation => reservation.Extent.Date.Year == year)
            .Aggregate(new ResourceSummary(ResourceType.Bedroom, 0, 0, Amount.Zero),
                (sum, reservation) => sum with
                {
                    ReservationCount = sum.ReservationCount + 1,
                    Nights = sum.Nights + reservation.Extent.Nights,
                    Income = sum.Income + reservation.Price!.Rent + reservation.Price.Cleaning
                });
        var banquetFacilitiesSummary = userOrder3.Reservations!.Where(reservation => reservation.Extent.Date.Year == year)
            .Aggregate(new ResourceSummary(ResourceType.BanquetFacilities, 0, 0, Amount.Zero),
                (sum, reservation) => sum with
                {
                    ReservationCount = sum.ReservationCount + 1,
                    Nights = sum.Nights + reservation.Extent.Nights,
                    Income = sum.Income + reservation.Price!.Rent + reservation.Price.Cleaning
                });
        yearlySummary.ResourceSummaries.Should().BeEquivalentTo(new[] { bedroomSummary, banquetFacilitiesSummary });
    }
}
