using FluentAssertions;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Shared.Core;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests;

public class DeleteTransaction : IClassFixture<SessionFixture>
{
    public DeleteTransaction(SessionFixture session) => Session = session;

    SessionFixture Session { get; }

    [Fact]
    public async Task DoublePayinTest()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        await Session.PayInAsync(userOrder.Payment!.PaymentId, userOrder.Payment.Amount);
        await Session.PayInAsync(userOrder.Payment!.PaymentId, userOrder.Payment.Amount);
        var userId = userOrder.UserInformation.UserId;
        var userTransactions = await Session.GetUserTransactionsAsync(userId);
        var transactionToDelete = userTransactions.Transactions.Last();
        await Session.DeleteTransactionAsync(transactionToDelete.TransactionId);
        var user = await Session.GetUserAsync(userId);
        userTransactions = await Session.GetUserTransactionsAsync(userId);
        var myTransactions = await Session.GetMyTransactionsAsync();
        var myBalance = userTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        user.Balance.Should().Be(Amount.Zero);
        user.Audits.Where(audit => audit.Type is UserAuditType.PayIn).Should().HaveCount(1);
        userTransactions.Transactions.Should().HaveCount(2);
        myTransactions.Transactions.Should().HaveCount(2);
        myBalance.Should().Be(Amount.Zero);
    }

    [Fact]
    public async Task DoublePayoutTest()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        await Session.SettleReservationAsync(Session.UserId(), userOrder.OrderId, 0);
        var creditors = await Session.GetCreditorsAsync();
        var creditor = creditors.Single(c => c.UserInformation.UserId == Session.UserId());
        await Session.PayOutAsync(creditor.UserInformation.UserId, creditor.Amount);
        await Session.PayOutAsync(creditor.UserInformation.UserId, creditor.Amount);
        var userId = userOrder.UserInformation.UserId;
        var userTransactions = await Session.GetUserTransactionsAsync(userId);
        var transactionToDelete = userTransactions.Transactions.Last();
        await Session.DeleteTransactionAsync(transactionToDelete.TransactionId);
        var user = await Session.GetUserAsync(userId);
        userTransactions = await Session.GetUserTransactionsAsync(userId);
        var myTransactions = await Session.GetMyTransactionsAsync();
        var myBalance = myTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        user.Balance.Should().Be(Amount.Zero);
        user.Audits.Where(audit => audit.Type is UserAuditType.PayOut).Should().HaveCount(1);
        userTransactions.Transactions.Should().HaveCount(4);
        myTransactions.Transactions.Should().HaveCount(4);
        myBalance.Should().Be(Amount.Zero);
    }
}
