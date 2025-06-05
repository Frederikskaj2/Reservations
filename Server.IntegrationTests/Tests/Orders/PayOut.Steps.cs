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

sealed partial class PayOut(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    State<MyOrderDto> order1;
    State<MyOrderDto> order2;
    State<CreditorDto> creditor;
    State<CreditorDto?> paidCreditor;

    MyOrderDto Order1 => order1.GetValue(nameof(Order1));
    MyOrderDto Order2 => order2.GetValue(nameof(Order2));
    CreditorDto Creditor => creditor.GetValue(nameof(Creditor));
    CreditorDto? PaidCreditor => paidCreditor.GetValue(nameof(PaidCreditor));

    async Task IScenarioSetUp.OnScenarioSetUp() => await session.UpdateLockBoxCodes();

    async Task GivenAResident() => await session.SignUpAndSignIn();

    async Task GivenAPaidOrder()
    {
        var getMyOrderResponse = await session.PlaceAndPayResidentOrder(new TestReservation(SeedData.Frederik.ResourceId));
        order1 = getMyOrderResponse.Order;
        await session.ConfirmOrders();
    }

    async Task GivenAnotherPaidOrder()
    {
        var getMyOrderResponse = await session.PlaceAndPayResidentOrder(new TestReservation(SeedData.BanquetFacilities.ResourceId, 2));
        order2 = getMyOrderResponse.Order;
        await session.ConfirmOrders();
    }

    async Task GivenTheOrderIsSettled() => await session.SettleReservation(Order1.OrderId, 0);

    async Task GivenTheOtherOrderIsSettled() => await session.SettleReservation(Order2.OrderId, 0);

    async Task WhenTheDepositIsRefunded(Amount missingAmount)
    {
        var getCreditorsResponse1 = await session.GetCreditors();
        creditor = getCreditorsResponse1.Creditors.Single(c => c.UserInformation.UserId == session.UserId());
        await session.PayOut(session.UserId(), Creditor.Payment.Amount - missingAmount);
        var getCreditorsResponse2 = await session.GetCreditors();
        paidCreditor = getCreditorsResponse2.Creditors.SingleOrDefault(c => c.UserInformation.UserId == session.UserId());
    }

    Task ThenTheCreditorHasAmount(Amount amount)
    {
        Creditor.Payment.Should().NotBeNull();
        Creditor.Payment.AccountNumber.Should().NotBeEmpty();
        Creditor.Payment.Amount.Should().Be(amount);
        return Task.CompletedTask;
    }

    Task ThenTheResidentIsNoLongerACreditor()
    {
        PaidCreditor.Should().BeNull();
        return Task.CompletedTask;
    }

    Task ThenThePaidCreditorHasAmount(Amount amount)
    {
        PaidCreditor.Should().NotBeNull();
        PaidCreditor!.Payment.Should().NotBeNull();
        PaidCreditor.Payment.AccountNumber.Should().NotBeEmpty();
        PaidCreditor.Payment.Amount.Should().Be(amount);
        return Task.CompletedTask;
    }

    async Task ThenTheResidentsBalanceIsAmount(Amount amount)
    {
        var myTransactionsResponse = await session.GetMyTransactions();
        var balance = myTransactionsResponse.Transactions.Sum(transaction => transaction.Amount);
        balance.Should().Be(amount);
    }
}
