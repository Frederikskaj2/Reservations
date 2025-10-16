using FluentAssertions;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Framework;
using LightBDD.XUnit2;
using NodaTime;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

sealed partial class MyOrders(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    State<MyOrderDto> upcomingOrder;
    State<GetMyOrdersResponse> getMyOrdersResponse;

    MyOrderDto UpcomingOrder => upcomingOrder.GetValue(nameof(MyOrderDto));
    GetMyOrdersResponse GetMyOrdersResponse => getMyOrdersResponse.GetValue(nameof(GetMyOrdersResponse));

    async Task IScenarioSetUp.OnScenarioSetUp()
    {
        session.NowOffset = Period.Zero;
        await session.UpdateLockBoxCodes();
    }

    async Task GivenResidentHasAHistoryOrder()
    {
        await session.SignUpAndSignIn();
        var getMyOrderResponse = await session.PlaceAndPayResidentOrder(new TestReservation(SeedData.Frederik.ResourceId));
        await session.RunConfirmOrders();
        await session.SettleReservation(getMyOrderResponse.Order.OrderId, 0);
    }

    async Task GivenResidentHasAnUpcomingOrder()
    {
        var getMyOrderResponse = await session.PlaceAndPayResidentOrder(new TestReservation(SeedData.Kaj.ResourceId, 1, TestReservationType.Monday));
        upcomingOrder = getMyOrderResponse.Order;
        await session.RunConfirmOrders();
    }

    async Task WhenTheResidentRetrievesOrders()
    {
        var offsetDays = Period.Between(session.TestStartDate, UpcomingOrder.Reservations.Single().Extent.Date, PeriodUnits.Days).Days - 1;
        session.NowOffset = Period.FromDays(offsetDays);
        getMyOrdersResponse = await session.GetMyOrders();
    }

    Task ThenAllOrdersAreRetrieved()
    {
        GetMyOrdersResponse.Orders.Should().HaveCount(2);
        return Task.CompletedTask;
    }

    Task ThenAHistoryOrderIsRetrieved()
    {
        GetMyOrdersResponse.Orders.Should().ContainEquivalentOf(new
        {
            IsHistoryOrder = true,
            Reservations = new[]
            {
                new
                {
                    Status = ReservationStatus.Settled,
                    LockBoxCodes = Array.Empty<DatedLockBoxCode>(),
                },
            },
        });
        return Task.CompletedTask;
    }

    Task ThenTheUpcomingOrderHasLockBoxCodes()
    {
        AssertionOptions.FormattingOptions.MaxDepth = 100;
        GetMyOrdersResponse.Orders.Should().ContainEquivalentOf(new
        {
            IsHistoryOrder = false,
            Reservations = new[]
            {
                new
                {
                    Status = ReservationStatus.Confirmed,
                    LockBoxCodes = new[]
                    {
                        new
                        {
                            UpcomingOrder.Reservations.Single().Extent.Date,
                        },
                    },
                },
            },
        });
        return Task.CompletedTask;
    }
}
