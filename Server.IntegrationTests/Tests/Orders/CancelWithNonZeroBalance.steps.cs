using FluentAssertions;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Users;
using LightBDD.Framework;
using LightBDD.XUnit2;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

sealed partial class CancelWithNonZeroBalance(SessionFixture session) : FeatureFixture, IClassFixture<SessionFixture>
{
    readonly Amount owedAmount = Amount.FromInt32(450);

    State<MyOrderDto> order;
    State<MyOrderDto> additionalOrder;
    State<UpdateMyOrderResponse> updateMyOrderResponse;
    State<Amount> cancellationFee;

    MyOrderDto Order => order.GetValue(nameof(Order));
    MyOrderDto AdditionalOrder => additionalOrder.GetValue(nameof(AdditionalOrder));
    UpdateMyOrderResponse UpdateMyOrderResponse => updateMyOrderResponse.GetValue(nameof(UpdateMyOrderResponse));
    Amount CancellationFee => cancellationFee.GetValue(nameof(CancellationFee));


    async Task GivenAResidentIsSignedIn() => await session.SignUpAndSignIn();

    async Task GivenResidentIsOwedMoney() =>
        await session.Reimburse(session.UserId(), IncomeAccount.Cleaning, "Cleaning", owedAmount);

    async Task GivenResidentOwesMoney()
    {
        var getMyOrderResponse = await session.PlaceResidentOrder(new TestReservation(SeedData.Kaj.ResourceId, 1, PriceGroup.Low));
        additionalOrder = getMyOrderResponse.Order;
    }

    async Task GivenAnOrder()
    {
        var getMyOrderResponse = await session.PlaceResidentOrder(new TestReservation(SeedData.Frederik.ResourceId, 1, PriceGroup.Low));
        order = getMyOrderResponse.Order;
    }

    async Task GivenAPaidOrder()
    {
        var getMyOrderResponse = await session.PlaceAndPayResidentOrder(new TestReservation(SeedData.Frederik.ResourceId, 1, PriceGroup.Low));
        order = getMyOrderResponse.Order;
        await session.RunConfirmOrders();
    }

    async Task WhenTheOrderIsCancelled() =>
        updateMyOrderResponse = await session.CancelResidentReservations(Order.OrderId, 0);

    Task ThenTheOrderHasACancellationFee()
    {
        var lineItem = UpdateMyOrderResponse.Order.AdditionalLineItems.Single();
        lineItem.Should().NotBeNull();
        lineItem.Type.Should().Be(LineItemType.CancellationFee);
        cancellationFee = lineItem.Amount;
        CancellationFee.Should().BeLessThan(Amount.Zero);
        return Task.CompletedTask;
    }

    async Task ThenTheResidentsBalanceIsTheOwedAmount()
    {
        var myTransactions = await session.GetMyTransactions();
        var balance = myTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        balance.Should().Be(owedAmount);
    }

    async Task ThenTheResidentsBalanceIsTheAmountPreviouslyOwed()
    {
        var myTransactions = await session.GetMyTransactions();
        var balance = myTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        balance.Should().Be(-AdditionalOrder.Price.Total());
    }

    async Task ThenTheResidentsBalanceIsTheOwedAmountAndTheRefund()
    {
        var myTransactions = await session.GetMyTransactions();
        var balance = myTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        balance.Should().Be(owedAmount + Order.Price.Total() + CancellationFee);
    }

    async Task ThenTheResidentIsACreditorForTheOwedAmount()
    {
        var getCreditorsResponse = await session.GetCreditors();
        getCreditorsResponse.Creditors.Should().ContainSingle(creditor => creditor.UserInformation.UserId == session.UserId())
            .Which.Payment.Amount.Should().Be(owedAmount);
    }

    async Task ThenTheResidentIsACreditorForTheOwedAmountAndTheRefund()
    {
        var getCreditorsResponse = await session.GetCreditors();
        getCreditorsResponse.Creditors.Should().ContainSingle(creditor => creditor.UserInformation.UserId == session.UserId())
            .Which.Payment.Amount.Should().Be(owedAmount + Order.Price.Total() + CancellationFee);
    }

    async Task ThenTheResidentIsNotACreditor()
    {
        var getCreditorsResponse = await session.GetCreditors();
        getCreditorsResponse.Creditors.Should().NotContain(creditor => creditor.UserInformation.UserId == session.UserId());
    }
}
