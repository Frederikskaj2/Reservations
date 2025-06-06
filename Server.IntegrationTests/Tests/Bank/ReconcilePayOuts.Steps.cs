﻿using FluentAssertions;
using Frederikskaj2.Reservations.Bank;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Users;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Framework;
using LightBDD.XUnit2;
using NodaTime;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Bank;

sealed partial class ReconcilePayOuts(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    State<CreditorDto> creditor;
    State<PayOutDto> payout;
    State<LocalDate> paymentDate;
    State<BankTransactionDto> importedBankTransaction;
    State<BankTransactionDto> reconciledBankTransaction;
    State<TransactionDto> payOutTransaction;

    CreditorDto Creditor => creditor.GetValue(nameof(Creditor));
    PayOutDto PayOut => payout.GetValue(nameof(PayOut));
    LocalDate PaymentDate => paymentDate.GetValue(nameof(PaymentDate));
    BankTransactionDto ImportedBankTransaction => importedBankTransaction.GetValue(nameof(ImportedBankTransaction));
    BankTransactionDto ReconciledBankTransaction => reconciledBankTransaction.GetValue(nameof(ReconciledBankTransaction));
    TransactionDto PayOutTransaction => payOutTransaction.GetValue(nameof(PayOutTransaction));

    async Task IScenarioSetUp.OnScenarioSetUp() => await session.UpdateLockBoxCodes();

    async Task GivenASettledOrder()
    {
        await session.SignUpAndSignIn();
        var getMyOrderResponse = await session.PlaceAndPayResidentOrder(new TestReservation(SeedData.Frederik.ResourceId));
        await session.ConfirmOrders();
        await session.SettleReservation(getMyOrderResponse.Order.OrderId, 0);
        var getCreditorsResponse = await session.GetCreditors();
        creditor = getCreditorsResponse.Creditors.Single(c => c.UserInformation.UserId == session.UserId());
    }

    async Task GivenAPayOutIsCreated()
    {
        var clientCreatePayOutResponse = await session.CreatePayOut(Creditor.UserInformation.UserId, Creditor.Payment.Amount);
        payout = clientCreatePayOutResponse.PayOut;
        paymentDate = PayOut.Timestamp.InZone(session.TimeZone).Date.PlusDays(1);
    }

    async Task GivenBankTransactionsAreImported() =>
        importedBankTransaction = await session.ImportBankTransaction(paymentDate, session.UserId(), -Creditor.Payment.Amount);

    async Task WhenTheTransactionIsReconciled()
    {
        var reconcilePayOutResponse = await session.ReconcilePayOut(ImportedBankTransaction.BankTransactionId, PayOut.PayOutId);
        reconciledBankTransaction = reconcilePayOutResponse.Transaction;
    }

    Task ThenTheBankTransactionIsReconciled()
    {
        ReconciledBankTransaction.Status.Should().Be(BankTransactionStatus.Reconciled);
        return Task.CompletedTask;
    }

    async Task ThenThePayOutIsDeleted()
    {
        var getPayOutsResponse = await session.GetPayOuts();
        getPayOutsResponse.PayOuts.Should().NotContainEquivalentOf(new { PayOut.PayOutId });
    }

    async Task ThenThePayOutAppearsOnTheResidentsAccountStatementThatHasABalanceOf0()
    {
        var getResidentTransactionsResponse = await session.GetResidentTransactions(session.UserId());
        payOutTransaction = getResidentTransactionsResponse.Transactions.Last();
        PayOutTransaction.Should().BeEquivalentTo(new { Date = PaymentDate, Activity = Activity.PayOut, Amount = -PayOut.Amount });
        var balance = getResidentTransactionsResponse.Transactions.Sum(transaction => transaction.Amount);
        balance.Should().Be(Amount.Zero);
    }

    async Task ThenTheReconciliationIsAudited()
    {
        var getUserResponse = await session.GetUser(session.UserId());
        getUserResponse.User.Audits.Should().ContainEquivalentOf(
            new { UserId = SeedData.AdministratorUserId, Type = UserAuditType.PayOut, PayOutTransaction.TransactionId });
    }
}
