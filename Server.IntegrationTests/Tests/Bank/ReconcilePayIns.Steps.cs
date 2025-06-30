using FluentAssertions;
using Frederikskaj2.Reservations.Bank;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Users;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Framework;
using LightBDD.XUnit2;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Bank;

sealed partial class ReconcilePayIns(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    State<MyOrderDto> order;
    State<BankTransactionDto> importedBankTransaction;
    State<BankTransactionDto> reconciledBankTransaction;
    State<GetMyTransactionsResponse> getMyTransactionsResponse;

    MyOrderDto Order => order.GetValue(nameof(Order));
    BankTransactionDto ImportedBankTransaction => importedBankTransaction.GetValue(nameof(ImportedBankTransaction));
    BankTransactionDto ReconciledBankTransaction => reconciledBankTransaction.GetValue(nameof(ReconciledBankTransaction));
    GetMyTransactionsResponse GetMyTransactionsResponse => getMyTransactionsResponse.GetValue(nameof(GetMyTransactionsResponse));

    async Task IScenarioSetUp.OnScenarioSetUp() => await session.UpdateLockBoxCodes();

    async Task GivenAnUnpaidOrder()
    {
        await session.SignUpAndSignIn();
        var getMyOrderResponse = await session.PlaceResidentOrder(new TestReservation(SeedData.Frederik.ResourceId));
        order = getMyOrderResponse.Order;
    }

    async Task GivenOrderHasBeenPaidAndBankTransactionsImported() =>
        importedBankTransaction = await session.ImportBankTransaction(session.CurrentDate, session.UserId(), Order.Payment!.Amount);

    async Task WhenThePayInIsReconciled()
    {
        var reconcilePayInResponse = await session.Reconcile(ImportedBankTransaction.BankTransactionId, Order.Payment!.PaymentId);
        reconciledBankTransaction = reconcilePayInResponse.Transaction;
    }

    Task ThenTheBankTransactionIsReconciled()
    {
        ReconciledBankTransaction.Status.Should().Be(BankTransactionStatus.Reconciled);
        return Task.CompletedTask;
    }

    async Task ThenTheResidentsBalanceIs0()
    {
        getMyTransactionsResponse = await session.GetMyTransactions();
        var balance = GetMyTransactionsResponse.Transactions.Select(transaction => transaction.Amount).Sum();
        balance.Should().Be(Amount.Zero);
    }

    Task ThenTheResidentHasNoOutstandingPayment()
    {
        GetMyTransactionsResponse.Payment.Should().BeNull();
        var balance = GetMyTransactionsResponse.Transactions.Select(transaction => transaction.Amount).Sum();
        balance.Should().BeGreaterOrEqualTo(Amount.Zero);
        return Task.CompletedTask;
    }
}
