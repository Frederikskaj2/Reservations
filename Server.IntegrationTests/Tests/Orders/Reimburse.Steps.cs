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

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

sealed partial class Reimburse(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    const string reimbursementDescription = "Cleaning";

    State<MyOrderDto> order;
    State<IEnumerable<PostingDto>> postings;
    State<IEnumerable<MyTransactionDto>> transactions;

    MyOrderDto Order => order.GetValue(nameof(Order));
    IEnumerable<PostingDto> Postings => postings.GetValue(nameof(Postings));
    IEnumerable<MyTransactionDto> Transactions => transactions.GetValue(nameof(Transactions));

    async Task IScenarioSetUp.OnScenarioSetUp() => await session.UpdateLockBoxCodes();

    async Task GivenAResidentIsSignedIn() => await session.SignUpAndSignIn();

    async Task GivenAPaidOrder()
    {
        var getMyOrderResponse = await session.PlaceAndPayResidentOrder(new TestReservation(SeedData.Frederik.ResourceId));
        order = getMyOrderResponse.Order;
        await session.ConfirmOrders();
    }

    async Task GivenTheOrderIsSettled() => await session.SettleReservation(Order.OrderId, 0);

    async Task WhenTheCleaningIsReimbursed() =>
        await session.Reimburse(session.UserId(), IncomeAccount.Cleaning, reimbursementDescription, Order.Price.Cleaning);

    async Task WhenThePostingsAreRetrieved()
    {
        var getPostingsResponse = await session.GetPostings(session.CurrentDate);
        postings = new(getPostingsResponse.Postings);
    }

    async Task WhenTheResidentsTransactionsAreRetrieved()
    {
        var getMyTransactionsResponse = await session.GetMyTransactions();
        transactions = new(getMyTransactionsResponse.Transactions);
    }

    async Task ThenTheResidentIsOwedTheDepositPlusTheReimbursedAmountForLackOfCleaning()
    {
        var getCreditorsResponse = await session.GetCreditors();
        getCreditorsResponse.Creditors.Should().ContainSingle(creditor => creditor.UserInformation.UserId == session.UserId())
            .Which.Payment.Amount.Should().Be(Order.Price.Deposit + Order.Price.Cleaning);
    }

    Task ThenTheResidentsBalanceIsTheDepositPlusTheReimbursedAmountForLackOfCleaning()
    {
        var balance = Transactions.Select(transaction => transaction.Amount).Sum();
        balance.Should().Be(Order.Price.Deposit + Order.Price.Cleaning);
        return Task.CompletedTask;
    }

    Task ThenTheResidentsLastTransactionIsDescribedAsCleaning()
    {
        var lastTransaction = Transactions.Last();
        lastTransaction.Description.Should().Be(reimbursementDescription);
        return Task.CompletedTask;
    }

    Task ThenFourPostingsAreReturned()
    {
        Postings.Should().HaveCount(4);
        return Task.CompletedTask;
    }

    Task ThenTheLastPostingIsTheReimbursement()
    {
        var posting = Postings.Last();
        posting.Should().BeEquivalentTo(
            new
            {
                Date = session.CurrentDate,
                Activity = Activity.Reimburse,
                session.User!.FullName,
                OrderId = (OrderId?) null,
                Amounts = new[]
                {
                    new { Account = PostingAccount.Income, Amount = Debit(Order.Price.Cleaning) },
                    new { Account = PostingAccount.AccountsPayable, Amount = Credit(Order.Price.Cleaning) },
                },
            });
        return Task.CompletedTask;
    }
}
