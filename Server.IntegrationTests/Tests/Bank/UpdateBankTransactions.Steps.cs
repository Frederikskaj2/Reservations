using FluentAssertions;
using Frederikskaj2.Reservations.Bank;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Framework;
using LightBDD.XUnit2;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Bank;

sealed partial class UpdateBankTransactions(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    State<BankTransactionDto> updatedTransaction;
    State<IEnumerable<BankTransactionDto>> retrievedTransactions;

    BankTransactionDto UpdatedTransaction => updatedTransaction.GetValue(nameof(UpdatedTransaction));
    IEnumerable<BankTransactionDto> RetrievedTransactions => retrievedTransactions.GetValue(nameof(RetrievedTransactions));

    async Task IScenarioSetUp.OnScenarioSetUp() => await session.ImportBankTransactions(csvContent);

    async Task WhenBankTransactionIsUpdated(BankTransactionId bankTransactionId, BankTransactionStatus status)
    {
        var updateBankTransactionResponse = await session.UpdateBankTransaction(bankTransactionId, status);
        updatedTransaction = updateBankTransactionResponse.Transaction;
    }

    async Task WhenBankTransactionsAreRetrieved()
    {
        var getBankTransactionsResponse = await session.GetBankTransactions(
            startDate: null, endDate: null, includeUnknown: true, includeIgnored: true, includeReconciled: true);
        retrievedTransactions = new(getBankTransactionsResponse.Transactions);
    }

    Task ThenUpdatedBankTransactionIs(BankTransactionDto transaction)
    {
        UpdatedTransaction.Should().Be(transaction);
        return Task.CompletedTask;
    }

    Task ThenRetrievedBankTransactionContains(BankTransactionDto expectedTransaction)
    {
        RetrievedTransactions.Should().Contain(expectedTransaction);
        return Task.CompletedTask;
    }
}
