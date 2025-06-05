using FluentAssertions;
using Frederikskaj2.Reservations.Bank;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Users;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Framework;
using LightBDD.XUnit2;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static Frederikskaj2.Reservations.Server.IntegrationTests.Harness.AmountFunctions;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Bank;

sealed partial class PostingsWithTooMuchPaidOut(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    readonly Amount excessAmount = Amount.FromInt32(123);

    State<MyOrderDto> order;
    State<CreditorDto> creditor;
    State<IEnumerable<PostingDto>> postings;
    State<IEnumerable<ResidentDto>> residents;
    State<IEnumerable<CreditorDto>> creditors;

    MyOrderDto Order => order.GetValue(nameof(Order));
    CreditorDto Creditor => creditor.GetValue(nameof(Creditor));
    IEnumerable<PostingDto> Postings => postings.GetValue(nameof(Postings));
    IEnumerable<ResidentDto> Residents => residents.GetValue(nameof(Residents));
    IEnumerable<CreditorDto> Creditors => creditors.GetValue(nameof(Creditors));

    async Task IScenarioSetUp.OnScenarioSetUp() => await session.UpdateLockBoxCodes();

    async Task GivenAResident() => await session.SignUpAndSignIn();

    async Task GivenASettledPaidOrder()
    {
        var getMyOrderResponse = await session.PlaceAndPayResidentOrder(new TestReservation(SeedData.Frederik.ResourceId));
        order = getMyOrderResponse.Order;
        await session.ConfirmOrders();
        await session.SettleReservation(getMyOrderResponse.Order.OrderId, 0);
        var getCreditorsResponse = await session.GetCreditors();
        creditor = getCreditorsResponse.Creditors.Single(c => c.UserInformation.UserId == session.UserId());
    }

    async Task GivenTooMuchDepositIsRefunded() =>
        await session.PayOut(session.UserId(), Creditor.Payment.Amount + excessAmount);

    async Task WhenThePostingsAreRetrieved()
    {
        var getPostingsResponse = await session.GetPostings(session.CurrentDate);
        postings = new(getPostingsResponse.Postings);
    }

    async Task WhenTheResidentsAreRetrieved()
    {
        var getResidentsResponse = await session.GetResidents();
        residents = new(getResidentsResponse.Residents);
    }

    async Task WhenTheCreditorsAreRetrieved()
    {
        var getCreditorsResponse = await session.GetCreditors();
        creditors = new(getCreditorsResponse.Creditors);
    }

    Task ThenFourPostingsAreReturned()
    {
        Postings.Should().HaveCount(4);
        return Task.CompletedTask;
    }

    Task ThenTheFirstPostingIsForPlacingTheOrder()
    {
        var posting = Postings.First();
        posting.Should().BeEquivalentTo(
            new
            {
                Date = session.CurrentDate,
                Activity = Activity.PlaceOrder,
                session.User!.FullName,
                Order.OrderId,
                Amounts = new[]
                {
                    new { Account = PostingAccount.Income, Amount = Credit(Order.Price.Total() - Order.Price.Deposit) },
                    new { Account = PostingAccount.Deposits, Amount = Credit(Order.Price.Deposit) },
                    new { Account = PostingAccount.AccountsReceivable, Amount = Debit(Order.Price.Total()) },
                },
            });
        return Task.CompletedTask;
    }

    Task ThenTheSecondPostingIsForPayingTheOrder()
    {
        var posting = Postings.ElementAt(1);
        posting.Should().BeEquivalentTo(
            new
            {
                Date = session.CurrentDate,
                Activity = Activity.PayIn,
                session.User!.FullName,
                OrderId = (OrderId?) null,
                Amounts = new[]
                {
                    new { Account = PostingAccount.AccountsReceivable, Amount = Credit(Order.Price.Total()) },
                    new { Account = PostingAccount.Bank, Amount = Debit(Order.Price.Total()) },
                },
            });
        return Task.CompletedTask;
    }

    Task ThenTheThirdPostingIsForSettlingTheOrder()
    {
        var deposit = Order.Price.Deposit;
        var posting = Postings.ElementAt(2);
        posting.Should().BeEquivalentTo(
            new
            {
                Date = session.CurrentDate,
                Activity = Activity.SettleReservation,
                session.User!.FullName,
                Order.OrderId,
                Amounts = new[]
                {
                    new { Account = PostingAccount.AccountsPayable, Amount = Credit(deposit) },
                    new { Account = PostingAccount.Deposits, Amount = Debit(deposit) },
                },
            });
        return Task.CompletedTask;
    }

    Task ThenTheFourthPostingIsForRefundingTheDeposit()
    {
        var posting = Postings.Last();
        posting.Should().BeEquivalentTo(
            new
            {
                Date = session.CurrentDate,
                Activity = Activity.PayOut,
                session.User!.FullName,
                OrderId = (OrderId?) null,
                Amounts = new[]
                {
                    new { Account = PostingAccount.AccountsReceivable, Amount = Debit(excessAmount) },
                    new { Account = PostingAccount.Bank, Amount = Credit(Creditor.Payment.Amount + excessAmount) },
                    new { Account = PostingAccount.AccountsPayable, Amount = Debit(Order.Price.Deposit) },
                },
            });
        return Task.CompletedTask;
    }

    Task ThenThereIsOneDebtor()
    {
        Residents.Should().ContainSingle();
        Residents.Single().Balance.Should().Be(-excessAmount);
        return Task.CompletedTask;
    }

    Task ThenThereAreNoCreditors()
    {
        Creditors.Should().BeEmpty();
        return Task.CompletedTask;
    }
}
