using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

[FeatureDescription(
    """
    As an administrator
    I want to be able to allow a resident to cancel one or more reservations without paying a fee
    So that the resident can change their order without further administrator intervention
    """)]
public partial class CancellationWithoutFee
{
    [Scenario]
    public Task AdministratorCanAllowCancellationWithoutAFee() =>
        Runner.RunScenarioAsync(
            GivenAResidentHasPlacedAndPaidAnOrder,
            AndGivenAnAdministratorHasAllowedTheResidentToCancelWithoutAFee,
            WhenTheResidentCancelsTheOrder,
            ThenTheOrderIsCancelled,
            AndTheResidentDoesNotHaveAnyReservedDaysInTheCalendar,
            AndTheFullPriceOfTheOrderIsOwedToTheResident);
}
