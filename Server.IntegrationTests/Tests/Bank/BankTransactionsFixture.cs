using FluentAssertions;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using LightBDD.Framework;
using LightBDD.XUnit2;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Bank;

public abstract class BankTransactionsFixture(SessionFixture session) : FeatureFixture, IClassFixture<SessionFixture>
{
    State<int> count;

    int Count => count.GetValue(nameof(Count));

    protected async Task WhenACsvFileFromBankIsImported(string csvContent)
    {
        var response = await session.ImportBankTransactions(csvContent);
        count = response.Count;
    }

    protected Task ThenANumberOfNewTransactionsAreCreated(int expectedCount)
    {
        Count.Should().Be(expectedCount);
        return Task.CompletedTask;
    }
}
