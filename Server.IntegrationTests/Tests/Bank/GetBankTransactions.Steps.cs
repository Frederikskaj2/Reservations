using FluentAssertions;
using Frederikskaj2.Reservations.Bank;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Framework;
using LightBDD.XUnit2;
using NodaTime;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Bank;

sealed partial class GetBankTransactions(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    State<IEnumerable<BankTransactionDto>> retrievedTransactions;

    IEnumerable<BankTransactionDto> RetrievedTransactions => retrievedTransactions.GetValue(nameof(RetrievedTransactions));

    async Task IScenarioSetUp.OnScenarioSetUp() => await session.ImportBankTransactions(csvContent);

    async Task WhenBankTransactionsAreRetrieved(LocalDate? startDate, LocalDate? endDate, bool includeUnknown, bool includeIgnored, bool includeReconciled)
    {
        var getBankTransactionsResponse = await session.GetBankTransactions(startDate, endDate, includeUnknown, includeIgnored, includeReconciled);
        retrievedTransactions = new(getBankTransactionsResponse.Transactions);
    }

    Task ThenBankTransactionsAreRetrieved(params BankTransactionDto[] expectedTransactions)
    {
        RetrievedTransactions.Should().BeEquivalentTo(expectedTransactions);
        return Task.CompletedTask;
    }
}
