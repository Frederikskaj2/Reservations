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
    public Task DeletePayOut() =>
        Runner.RunScenarioAsync(
            GivenASettledOrder,
            WhenAPayOutIsCreated,
            WhenThePayOutIsDeleted,
            WhenPayOutsAreRetrieved,
            ThenTheRetrievedPayOutsDoesNotContainThePayout);

    [Scenario]
    public Task OptimisticConcurrency() =>
        Runner.RunScenarioAsync(
            GivenASettledOrder,
            WhenAPayOutIsCreated,
            WhenThePayOutIsDeletedWithInvalidETag,
            WhenPayOutsAreRetrieved,
            ThenTheRetrievedPayOutsContainThePayout,
            ThenTheHttpStatusIsPreconditionFailed);
}
