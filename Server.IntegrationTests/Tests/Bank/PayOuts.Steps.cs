using FluentAssertions;
using Frederikskaj2.Reservations.Bank;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Framework;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Bank;

sealed partial class PayOuts(SessionFixture session) : PayOutsFixture(session), IScenarioSetUp
{
    const string note = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";
    const string accountNumber = "1234-1234567890";

    State<HttpResponseMessage> httpResponseMessage;
    State<IEnumerable<CreditorDto>> creditors;

    HttpResponseMessage HttpResponseMessage => httpResponseMessage.GetValue(nameof(HttpResponseMessage));
    IEnumerable<CreditorDto> Creditors => creditors.GetValue(nameof(Creditors));

    async Task IScenarioSetUp.OnScenarioSetUp() => await Session.UpdateLockBoxCodes();

    async Task WhenAnotherPayOutIsCreated() =>
        httpResponseMessage = await Session.CreatePayOutRaw(Creditor.UserInformation.UserId, Creditor.Payment.AccountNumber, Creditor.Payment.Amount);

    async Task WhenANoteIsAdded() => await Session.AddPayOutNote(PayOut.PayOutId, note);

    async Task WhenTheAccountNumberIsUpdated() => await Session.UpdatePayOutAccountNumber(PayOut.PayOutId, accountNumber);

    async Task WhenThePayOutIsCancelled() => await Session.CancelPayOut(PayOut.PayOutId);

    async Task WhenCreditorsAreRetrieved()
    {
        var getCreditorsResponse = await Session.GetCreditors();
        creditors = new(getCreditorsResponse.Creditors);
    }

    Task ThenThePayOutIsCreated()
    {
        PayOut.UserIdentity.Should().BeEquivalentTo(Creditor.UserInformation);
        PayOut.PaymentId.Should().BeEquivalentTo(Creditor.Payment.PaymentId);
        PayOut.AccountNumber.Should().BeEquivalentTo(Creditor.Payment.AccountNumber);
        PayOut.Amount.Should().BeEquivalentTo(Creditor.Payment.Amount);
        PayOut.Status.Should().Be(PayOutStatus.InProgress);
        return Task.CompletedTask;
    }

    Task ThenTheRetrievedPayOutsContainThePayout()
    {
        RetrievedPayOuts.Should().ContainEquivalentOf(new { PayOut.PayOutId, Status = PayOutStatus.InProgress, DelayedDays = (int?) null });
        return Task.CompletedTask;
    }

    Task ThenTheRecentlyCancelledPayOutIsStillIncluded()
    {
        RetrievedPayOuts.Should().ContainEquivalentOf(new { PayOut.PayOutId, Status = PayOutStatus.Cancelled, DelayedDays = (int?) null });
        return Task.CompletedTask;
    }

    Task ThenTheRetrievedPayOutContainsThePayout()
    {
        RetrievedPayOut.PayOutId.Should().Be(PayOut.PayOutId);
        RetrievedPayOut.UserIdentity.Should().BeEquivalentTo(Creditor.UserInformation);
        RetrievedPayOut.PaymentId.Should().BeEquivalentTo(Creditor.Payment.PaymentId);
        RetrievedPayOut.AccountNumber.Should().BeEquivalentTo(Creditor.Payment.AccountNumber);
        RetrievedPayOut.Amount.Should().BeEquivalentTo(Creditor.Payment.Amount);
        RetrievedPayOut.Status.Should().Be(PayOutStatus.InProgress);
        RetrievedPayOut.Notes.Should().BeEmpty();
        RetrievedPayOut.Audits.Should().ContainSingle().Which.Should().BeEquivalentTo(
            new { UserId = SeedData.AdministratorUserId, Type = PayOutAuditType.Create });
        RetrievedPayOut.DelayedDays.Should().BeNull();
        return Task.CompletedTask;
    }

    Task ThenTheRetrievedPayOutHasTheNote()
    {
        RetrievedPayOut.Notes.Should().ContainSingle().Which.Should().BeEquivalentTo(new { UserId = SeedData.AdministratorUserId, Note = note });
        RetrievedPayOut.Audits.Should().BeEquivalentTo([
            new { UserId = SeedData.AdministratorUserId, Type = PayOutAuditType.Create },
            new { UserId = SeedData.AdministratorUserId, Type = PayOutAuditType.AddNote },
        ]);
        return Task.CompletedTask;
    }

    Task ThenTheRetrievedPayOutHasTheNewAccountNumber()
    {
        RetrievedPayOut.AccountNumber.Should().Be(accountNumber);
        RetrievedPayOut.Audits.Should().BeEquivalentTo([
            new { UserId = SeedData.AdministratorUserId, Type = PayOutAuditType.Create },
            new { UserId = SeedData.AdministratorUserId, Type = PayOutAuditType.UpdateAccountNumber },
        ]);
        return Task.CompletedTask;
    }

    Task ThenTheRetrievedPayOutIsCancelled()
    {
        RetrievedPayOut.Status.Should().Be(PayOutStatus.Cancelled);
        RetrievedPayOut.Audits.Should().BeEquivalentTo([
            new { UserId = SeedData.AdministratorUserId, Type = PayOutAuditType.Create },
            new { UserId = SeedData.AdministratorUserId, Type = PayOutAuditType.Cancel },
        ]);
        return Task.CompletedTask;
    }

    Task ThenTheServerRespondsWithConflict()
    {
        HttpResponseMessage.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
        return Task.CompletedTask;
    }

    Task ThenTheResidentIsNotACreditor()
    {
        Creditors.Should().NotContain(c => c.UserInformation.UserId == Session.UserId());
        return Task.CompletedTask;
    }
}
