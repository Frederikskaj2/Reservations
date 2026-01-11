using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Bank;

[FeatureDescription(
    """
    As an administrator
    I want to keep track of pay-outs
    So that residents are refunded their deposits
    """)]
public partial class PayOuts
{
    [Scenario]
    public Task CreatePayOut() =>
        Runner.RunScenarioAsync(
            GivenASettledOrder,
            WhenAPayOutIsCreated,
            WhenPayOutsAreRetrieved,
            ThenThePayOutIsCreated,
            ThenTheRetrievedPayOutsContainThePayout);

    [Scenario]
    public Task GetPayOut() =>
        Runner.RunScenarioAsync(
            GivenASettledOrder,
            WhenAPayOutIsCreated,
            WhenThePayOutIsRetrieved,
            ThenTheRetrievedPayOutContainsThePayout);

    [Scenario]
    public Task AddNote() =>
        Runner.RunScenarioAsync(
            GivenASettledOrder,
            WhenAPayOutIsCreated,
            WhenANoteIsAdded,
            WhenThePayOutIsRetrieved,
            ThenTheRetrievedPayOutHasTheNote);

    [Scenario]
    public Task UpdateAccountNumber() =>
        Runner.RunScenarioAsync(
            GivenASettledOrder,
            WhenAPayOutIsCreated,
            WhenTheAccountNumberIsUpdated,
            WhenThePayOutIsRetrieved,
            ThenTheRetrievedPayOutHasTheNewAccountNumber);

    [Scenario]
    public Task CancelPayOut() =>
        Runner.RunScenarioAsync(
            GivenASettledOrder,
            WhenAPayOutIsCreated,
            WhenThePayOutIsCancelled,
            WhenPayOutsAreRetrieved,
            WhenThePayOutIsRetrieved,
            ThenTheRecentlyCancelledPayOutIsStillIncluded,
            ThenTheRetrievedPayOutIsCancelled);

    [Scenario]
    public Task OnlyOnePayOutPerResident() =>
        Runner.RunScenarioAsync(
            GivenASettledOrder,
            WhenAPayOutIsCreated,
            WhenAnotherPayOutIsCreated,
            ThenTheServerRespondsWithConflict);

    [Scenario]
    public Task AResidentWithAnInProgressPayOutIsNotACreditor() =>
        Runner.RunScenarioAsync(
            GivenASettledOrder,
            WhenAPayOutIsCreated,
            WhenCreditorsAreRetrieved,
            ThenTheResidentIsNotACreditor);
}
