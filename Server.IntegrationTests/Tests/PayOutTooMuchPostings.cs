using FluentAssertions;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Shared.Core;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static Frederikskaj2.Reservations.Server.IntegrationTests.Harness.AmountFunctions;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests;

public class PayOutTooMuchPostings : IClassFixture<SessionFixture>
{
    public PayOutTooMuchPostings(SessionFixture session) => Session = session;

    SessionFixture Session { get; }

    [Fact]
    public async Task Test()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var deposit = userOrder.Reservations!.Single().Price!.Deposit;
        await Session.SettleReservationAsync(Session.UserId(), userOrder.OrderId, 0);
        var creditor = (await Session.GetCreditorsAsync()).Single(c => c.UserInformation.UserId == Session.UserId());
        var excessAmount = Amount.FromInt32(123);
        var paidAmount = creditor.Amount + excessAmount;
        await Session.PayOutAsync(creditor.UserInformation.UserId, paidAmount);
        var debtors = await Session.GetDebtorsAsync();
        var creditors = await Session.GetCreditorsAsync();
        var postings = await Session.GetPostingsAsync(Session.CurrentDate);
        postings.Should().HaveCount(4);
        var placeOrderPosting = postings.First();
        var payInPosting = postings.Skip(1).First();
        var settlePosting = postings.Skip(2).First();
        var payOutPosting = postings.Last();
        placeOrderPosting.Date.Should().Be(Session.CurrentDate);
        placeOrderPosting.Activity.Should().Be(Activity.PlaceOrder);
        placeOrderPosting.FullName.Should().Be(Session.User!.FullName);
        placeOrderPosting.OrderId.Should().Be(userOrder.OrderId);
        placeOrderPosting.Amounts.Should().HaveCount(3);
        placeOrderPosting.Amounts.Should().Contain(new AccountAmount(PostingAccount.Income, Credit(userOrder.Price.Total() - userOrder.Price.Deposit)));
        placeOrderPosting.Amounts.Should().Contain(new AccountAmount(PostingAccount.Deposits, Credit(userOrder.Price.Deposit)));
        placeOrderPosting.Amounts.Should().Contain(new AccountAmount(PostingAccount.AccountsReceivable, Debit(userOrder.Price.Total())));
        payInPosting.Date.Should().Be(Session.CurrentDate);
        payInPosting.Activity.Should().Be(Activity.PayIn);
        payInPosting.FullName.Should().Be(Session.User!.FullName);
        payInPosting.OrderId.Should().BeNull();
        payInPosting.Amounts.Should().HaveCount(2);
        payInPosting.Amounts.Should().Contain(new AccountAmount(PostingAccount.AccountsReceivable, Credit(userOrder.Price.Total())));
        payInPosting.Amounts.Should().Contain(new AccountAmount(PostingAccount.Bank, Debit(userOrder.Price.Total())));
        settlePosting.Date.Should().Be(Session.CurrentDate);
        settlePosting.Activity.Should().Be(Activity.SettleReservation);
        settlePosting.FullName.Should().Be(Session.User!.FullName);
        settlePosting.OrderId.Should().Be(userOrder.OrderId);
        settlePosting.Amounts.Should().HaveCount(2);
        settlePosting.Amounts.Should().Contain(new AccountAmount(PostingAccount.AccountsPayable, Credit(deposit)));
        settlePosting.Amounts.Should().Contain(new AccountAmount(PostingAccount.Deposits, Debit(deposit)));
        payOutPosting.Date.Should().Be(Session.CurrentDate);
        payOutPosting.Activity.Should().Be(Activity.PayOut);
        payOutPosting.FullName.Should().Be(Session.User!.FullName);
        payOutPosting.OrderId.Should().BeNull();
        payOutPosting.Amounts.Should().HaveCount(3);
        payOutPosting.Amounts.Should().Contain(new AccountAmount(PostingAccount.AccountsReceivable, Debit(excessAmount)));
        payOutPosting.Amounts.Should().Contain(new AccountAmount(PostingAccount.Bank, Credit(paidAmount)));
        payOutPosting.Amounts.Should().Contain(new AccountAmount(PostingAccount.AccountsPayable, Debit(deposit)));
        debtors.Should().HaveCount(1);
        debtors.Single().Amount.Should().Be(excessAmount);
        creditors.Should().BeEmpty();
    }
}
