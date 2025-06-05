using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.LockBox;

[FeatureDescription(
    """
    As a lock box code maintainer
    I want to retrieve a list of lock box codes
    So I can know the current and future lock box codes
    """)]
public partial class LockBoxCodes
{
    [Scenario]
    public Task InitializeLockBoxCodes() =>
        Runner.RunScenarioAsync(
            WhenTheLockBoxCodesAreRetrieved,
            ThenAListOfLockBoxCodesIsReturned);

    [Scenario]
    public Task LockBoxCodesChangeEveryWeek() =>
        Runner.RunScenarioAsync(
            WhenTheLockBoxCodesAreRetrieved,
            WhenAWeekPasses,
            WhenTheJobToUpdateLockBoxCodesExecutes,
            WhenTheLockBoxCodesAreRetrievedAgain,
            ThenTheOutdatedLockBoxCodesNoLongerExistAndNewLockBoxCodesAreGenerated);

    [Scenario]
    public Task SendLockBoxCodesEmail() =>
        Runner.RunScenarioAsync(
            WhenTheLockBoxCodesAreSent,
            ThenAnEmailWithAListOfLockBoxCodesIsSent);
}
