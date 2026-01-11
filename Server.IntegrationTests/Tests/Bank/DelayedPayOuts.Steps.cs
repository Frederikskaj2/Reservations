using FluentAssertions;
using Frederikskaj2.Reservations.Bank;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using LightBDD.Core.Extensibility.Execution;
using NodaTime;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Bank;

sealed partial class DelayedPayOuts(SessionFixture session) : PayOutsFixture(session), IScenarioSetUp
{
    async Task IScenarioSetUp.OnScenarioSetUp()
    {
        Session.NowOffset = Period.Zero;
        await Session.UpdateLockBoxCodes();
    }

    async Task WhenBankTransactionsAreImported() =>
        await Session.ImportBankTransaction(Session.CurrentDate, Session.UserId(), Creditor.Payment.Amount);

    Task WhenTimePasses()
    {
        Session.NowOffset += Period.FromDays(7);
        return Task.CompletedTask;
    }

    Task ThenThePayOutIsDelayed()
    {
        RetrievedPayOut.DelayedDays.Should().BeGreaterThan(1);
        RetrievedPayOuts.Should().ContainEquivalentOf(new { PayOut.PayOutId, Status = PayOutStatus.InProgress, RetrievedPayOut.DelayedDays });
        return Task.CompletedTask;
    }
}
