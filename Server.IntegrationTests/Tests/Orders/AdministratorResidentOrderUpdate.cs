using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

[FeatureDescription(
    """
    As an administrator
    I want to be able to cancel a resident's reservation
    So that it's possible to cancel a reservation on behalf of the resident
    """)]
public partial class AdministratorResidentOrderUpdate
{
    [Scenario]
    public Task AdministratorCanCancelAReservation() =>
        Runner.RunScenarioAsync(
            GivenAResidentHasPlacedAnOrder,
            WhenTheAdministratorCancelsTheReservation,
            ThenTheOrderIsCancelled,
            AndTheCancellationIsAudited,
            AndTheResidentDoesNotHaveAnyReservedDaysInTheCalendar,
            AndTheBalanceOfTheResidentIsZero,
            AndTheResidentIsSentAnEmailAboutTheCancellation);
}
