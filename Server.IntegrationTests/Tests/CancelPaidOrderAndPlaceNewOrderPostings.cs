using FluentAssertions;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Shared.Core;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static Frederikskaj2.Reservations.Server.IntegrationTests.Harness.AmountFunctions;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests;

public class CancelPaidOrderAndPlaceNewOrderPostings : IClassFixture<SessionFixture>
{
    public CancelPaidOrderAndPlaceNewOrderPostings(SessionFixture session) => Session = session;

    SessionFixture Session { get; }

    [Fact]
    public async Task Test()
    {
        // This test verifies that a resident cannot have non-zero balances on both accounts receivable and accounts payable. After cancelling a paid order
        // the resident will have a credit balance on accounts payable. However, placing a new order (that costs more than what is owed to the resident)
        // will bring the balance of accounts payable to zero and a create a debit balance on accounts receivables with the net amount the resident owes us.
        await Session.SignUpAndSignInAsync();
        var userOrder1 = await Session.UserPlaceAndPayOrderAsync(
            new TestReservation(TestData.BanquetFacilities.ResourceId, 2), new TestReservation(TestData.Frederik.ResourceId));
        var reservationToCancel = userOrder1.Reservations!.First();
        var updateMyOrderResult = await Session.UserCancelReservationsAsync(userOrder1.OrderId, 0);
        var fee = -updateMyOrderResult.Order!.AdditionalLineItems.Single().Amount;
        var order1Debtors = await Session.GetDebtorsAsync();
        var order1Creditors = await Session.GetCreditorsAsync();
        // Reorder the cancelled reservation.
        var userOrder2 = await Session.UserPlaceOrderAsync(reservationToCancel);
        var order2Debtors = await Session.GetDebtorsAsync();
        var order2Creditors = await Session.GetCreditorsAsync();
        var postings = await Session.GetPostingsAsync(Session.CurrentDate);
        postings.Should().HaveCount(4);
        var placeOrder1Posting = postings.First();
        var payInPosting = postings.Skip(1).First();
        var cancelPosting = postings.Skip(2).First();
        var placeOrder2Posting = postings.Last();
        fee.Should().BeGreaterThan(Amount.Zero);
        placeOrder1Posting.Date.Should().Be(Session.CurrentDate);
        placeOrder1Posting.Activity.Should().Be(Activity.PlaceOrder);
        placeOrder1Posting.FullName.Should().Be(Session.User!.FullName);
        placeOrder1Posting.OrderId.Should().Be(userOrder1.OrderId);
        placeOrder1Posting.Amounts.Should().HaveCount(3);
        placeOrder1Posting.Amounts.Should().Contain(new AccountAmount(PostingAccount.Income, Credit(userOrder1.Price.Total() - userOrder1.Price.Deposit)));
        placeOrder1Posting.Amounts.Should().Contain(new AccountAmount(PostingAccount.Deposits, Credit(userOrder1.Price.Deposit)));
        placeOrder1Posting.Amounts.Should().Contain(new AccountAmount(PostingAccount.AccountsReceivable, Debit(userOrder1.Price.Total())));
        payInPosting.Date.Should().Be(Session.CurrentDate);
        payInPosting.Activity.Should().Be(Activity.PayIn);
        payInPosting.FullName.Should().Be(Session.User!.FullName);
        payInPosting.OrderId.Should().BeNull();
        payInPosting.Amounts.Should().HaveCount(2);
        payInPosting.Amounts.Should().Contain(new AccountAmount(PostingAccount.AccountsReceivable, Credit(userOrder1.Price.Total())));
        payInPosting.Amounts.Should().Contain(new AccountAmount(PostingAccount.Bank, Debit(userOrder1.Price.Total())));
        cancelPosting.Date.Should().Be(Session.CurrentDate);
        cancelPosting.Activity.Should().Be(Activity.UpdateOrder);
        cancelPosting.FullName.Should().Be(Session.User!.FullName);
        cancelPosting.OrderId.Should().Be(userOrder1.OrderId);
        cancelPosting.Amounts.Should().HaveCount(3);
        cancelPosting.Amounts.Should().Contain(new AccountAmount(PostingAccount.Income, Debit(reservationToCancel.Price!.Total() - reservationToCancel.Price.Deposit - fee)));
        cancelPosting.Amounts.Should().Contain(new AccountAmount(PostingAccount.Deposits, Debit(reservationToCancel.Price.Deposit)));
        cancelPosting.Amounts.Should().Contain(new AccountAmount(PostingAccount.AccountsPayable, Credit(reservationToCancel.Price.Total() - fee)));
        placeOrder2Posting.Date.Should().Be(Session.CurrentDate);
        placeOrder2Posting.Activity.Should().Be(Activity.PlaceOrder);
        placeOrder2Posting.FullName.Should().Be(Session.User!.FullName);
        placeOrder2Posting.OrderId.Should().Be(userOrder2.OrderId);
        placeOrder2Posting.Amounts.Should().HaveCount(4);
        placeOrder2Posting.Amounts.Should().Contain(new AccountAmount(PostingAccount.Income, Credit(userOrder2.Price.Total() - userOrder2.Price.Deposit)));
        placeOrder2Posting.Amounts.Should().Contain(new AccountAmount(PostingAccount.Deposits, Credit(userOrder2.Price.Deposit)));
        placeOrder2Posting.Amounts.Should().Contain(new AccountAmount(PostingAccount.AccountsReceivable, Debit(fee)));
        placeOrder2Posting.Amounts.Should().Contain(new AccountAmount(PostingAccount.AccountsPayable, Debit(reservationToCancel.Price.Total() - fee)));
        order1Debtors.Should().HaveCount(1);
        order1Debtors.Single().Amount.Should().Be(Amount.Zero);
        order1Creditors.Should().HaveCount(1);
        order1Creditors.Single().Amount.Should().Be(reservationToCancel.Price!.Total() - fee);
        order2Debtors.Should().HaveCount(1);
        order2Debtors.Single().Amount.Should().Be(fee);
        order2Creditors.Should().BeEmpty();
    }
}
