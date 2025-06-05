using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

[FeatureDescription(
    """
    As an administrator
    I want to view yearly summary reports
    So I can assess the usage and income of the shared house on a yearly basis
    """)]
public partial class YearlySummaryWithNoOrders
{
    [Scenario]
    public Task EmptyYearlySummary() =>
        Runner.RunScenarioAsync(
            GivenNoOrders,
            WhenTheYearlySummaryRangeIsRetrieved,
            WhenTheYearlySummaryIsRetrieved,
            ThenTheYearRangeIsThisYear,
            ThenTheYearlySummaryIsEmpty);
}
