using FluentAssertions;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Shared.Core;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static Frederikskaj2.Reservations.Server.IntegrationTests.Harness.AmountFunctions;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests;

public class PayInTooMuchPostings : IClassFixture<SessionFixture>
{
    public PayInTooMuchPostings(SessionFixture session) => Session = session;

    SessionFixture Session { get; }

    [Fact]
    public async Task Test()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var excessAmount = Amount.FromInt32(123);
        await Session.PayInAsync(userOrder.Payment!.PaymentId, userOrder.Payment.Amount + excessAmount);
        var debtors = await Session.GetDebtorsAsync();
        var creditors = await Session.GetCreditorsAsync();
        var postings = await Session.GetPostingsAsync(Session.CurrentDate);
        postings.Should().HaveCount(2);
        var placeOrderPosting = postings.First();
        var payInPosting = postings.Skip(1).First();
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
        payInPosting.Amounts.Should().HaveCount(3);
        payInPosting.Amounts.Should().Contain(new AccountAmount(PostingAccount.AccountsReceivable, Credit(userOrder.Price.Total())));
        payInPosting.Amounts.Should().Contain(new AccountAmount(PostingAccount.Bank, Debit(userOrder.Price.Total() + excessAmount)));
        payInPosting.Amounts.Should().Contain(new AccountAmount(PostingAccount.AccountsPayable, Credit(excessAmount)));
        debtors.Should().HaveCount(1);
        debtors.Single().Amount.Should().Be(Amount.Zero);
        creditors.Should().HaveCount(1);
        creditors.Single().Amount.Should().Be(excessAmount);
    }
}
