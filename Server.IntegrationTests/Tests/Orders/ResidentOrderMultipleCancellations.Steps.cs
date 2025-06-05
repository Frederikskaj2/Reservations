using FluentAssertions;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Users;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Framework;
using LightBDD.XUnit2;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

sealed partial class ResidentOrderMultipleCancellations(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    State<MyOrderDto> order;
    State<UpdateMyOrderResponse> updateMyOrderResponse1;
    State<UpdateMyOrderResponse> updateMyOrderResponse2;
    State<Amount> cancellationFee1;
    State<Amount> cancellationFee2;

    MyOrderDto Order => order.GetValue(nameof(Order));
    UpdateMyOrderResponse UpdateMyOrderResponse1 => updateMyOrderResponse1.GetValue(nameof(UpdateMyOrderResponse1));
    UpdateMyOrderResponse UpdateMyOrderResponse2 => updateMyOrderResponse2.GetValue(nameof(UpdateMyOrderResponse2));
    Amount CancellationFee1 => cancellationFee1.GetValue(nameof(CancellationFee1));
    Amount CancellationFee2 => cancellationFee1.GetValue(nameof(CancellationFee2));

    async Task IScenarioSetUp.OnScenarioSetUp() => await session.UpdateLockBoxCodes();

    async Task GivenAResidentIsSignedIn() => await session.SignUpAndSignIn();

    async Task GivenAnOrderIsPlacedAndPaid()
    {
        var getMyOrderResponse = await session.PlaceAndPayResidentOrder(
            new TestReservation(SeedData.BanquetFacilities.ResourceId, 2),
            new TestReservation(SeedData.Frederik.ResourceId),
            new TestReservation(SeedData.Kaj.ResourceId, 2),
            new TestReservation(SeedData.Frederik.ResourceId, 3));
        order = getMyOrderResponse.Order;
        await session.ConfirmOrders();
    }

    async Task WhenAReservationsIsCancelled() => updateMyOrderResponse1 = await session.CancelResidentReservations(Order.OrderId, 0);

    async Task WhenTwoMoreReservationsAreCancelled() => updateMyOrderResponse2 = await session.CancelResidentReservations(Order.OrderId, 1, 2);

    Task ThenTheFirstCancellationHasAFee()
    {
        var lineItem = UpdateMyOrderResponse1.Order.AdditionalLineItems.Single();
        lineItem.Should().NotBeNull();
        lineItem.Type.Should().Be(LineItemType.CancellationFee);
        cancellationFee1 = lineItem.Amount;
        CancellationFee1.Should().BeLessThan(Amount.Zero);
        return Task.CompletedTask;
    }

    Task ThenTheSecondCancellationHasAFee()
    {
        var lineItem = UpdateMyOrderResponse2.Order.AdditionalLineItems.Skip(1).Single();
        lineItem.Should().NotBeNull();
        lineItem.Type.Should().Be(LineItemType.CancellationFee);
        cancellationFee2 = lineItem.Amount;
        CancellationFee2.Should().BeLessThan(Amount.Zero);
        return Task.CompletedTask;
    }

    Task ThenTheCancellationFeesAreTheSame()
    {
        CancellationFee1.Should().Be(CancellationFee2);
        return Task.CompletedTask;
    }

    async Task ThenTheResidentsBalanceIsThePriceOfTheCancelledReservationsMinusTheCancellationFees()
    {
        var getMyTransactionsResponse = await session.GetMyTransactions();
        var balance = getMyTransactionsResponse.Transactions.Select(transaction => transaction.Amount).Sum();
        var price1 = UpdateMyOrderResponse1.Order.Reservations.First().Price!.Total();
        var price2 = UpdateMyOrderResponse2.Order.Reservations.Skip(1).First().Price!.Total() +
                     UpdateMyOrderResponse2.Order.Reservations.Skip(2).First().Price!.Total();
        balance.Should().Be(price1 + cancellationFee1 + price2 + cancellationFee2);
    }

    async Task ThenTwoCancellationsHaveBeenAudited()
    {
        var getOrderResponse = await session.GetOrder(Order.OrderId);
        getOrderResponse.Order.Audits.Select(audit => audit.Type)
            .Should()
            .Equal(OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder, OrderAuditType.CancelReservation, OrderAuditType.CancelReservation);
    }
}
