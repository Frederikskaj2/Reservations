using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Bank;

[FeatureDescription(
    """
    As an administrator
    I want to notice when pay-outs are delayed
    So that pay-outs can be fixed and/or restarted when there's a hiccup in the pay-out process
    """)]
public partial class DelayedPayOuts
{
    [Scenario]
    public Task DelayedPayOut() =>
        Runner.RunScenarioAsync(
            GivenASettledOrder,
            WhenAPayOutIsCreated,
            WhenTimePasses,
            WhenBankTransactionsAreImported,
            WhenPayOutsAreRetrieved,
            WhenThePayOutIsRetrieved,
            ThenThePayOutIsDelayed);
}
