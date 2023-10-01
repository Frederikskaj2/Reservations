using FluentAssertions;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Shared.Core;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests;

public class Reimburse : IClassFixture<SessionFixture>
{
    public Reimburse(SessionFixture session) => Session = session;

    SessionFixture Session { get; }
    [Fact]
    public async Task ReimburseCleaning()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var deposit = userOrder.Reservations!.Single().Price!.Deposit;
        var cleaning = userOrder.Price.Cleaning;
        await Session.SettleReservationAsync(Session.UserId(), userOrder.OrderId, 0);
        await Session.ReimburseAsync(Session.UserId(), IncomeAccount.Cleaning, "Cleaning", cleaning);
        var user = await Session.GetUserAsync(Session.UserId());
        var creditors = await Session.GetCreditorsAsync();
        var creditor = creditors.Single(c => c.UserInformation.UserId == Session.UserId());
        var postings = await Session.GetPostingsAsync(Session.CurrentDate);
        var myTransactions = await Session.GetMyTransactionsAsync();
        var myBalance = myTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        creditor.Amount.Should().Be(deposit + cleaning);
        postings.Should().HaveCount(4);
        var posting = postings.ElementAt(3);
        posting.Activity.Should().Be(Activity.Reimburse);
        posting.Amounts.Should().Equal(new AccountAmount[] { new(PostingAccount.Income, cleaning), new(PostingAccount.AccountsPayable, -cleaning) });
        myBalance.Should().Be(deposit + cleaning);
        user.Audits.Select(audit => audit.Type).Should().Equal(
            UserAuditType.SignUp, UserAuditType.ConfirmEmail, UserAuditType.SetAccountNumber, UserAuditType.CreateOrder, UserAuditType.PayIn,
            UserAuditType.Reimburse);
    }
}
