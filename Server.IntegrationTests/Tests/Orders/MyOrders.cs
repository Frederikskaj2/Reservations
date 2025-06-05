using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

[FeatureDescription(
    """
    As a resident
    I want to be able to get all my orders
    So that I can view current and past orders
    """)]
public partial class MyOrders
{
    [Scenario]
    public Task ResidentGetsOrders() =>
        Runner.RunScenarioAsync(
            GivenResidentHasAHistoryOrder,
            GivenResidentHasAnUpcomingOrder,
            WhenTheResidentRetrievesOrders,
            ThenAllOrdersAreRetrieved,
            ThenAHistoryOrderIsRetrieved,
            ThenTheUpcomingOrderHasLockBoxCodes);
}
