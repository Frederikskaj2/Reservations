using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

[FeatureDescription(
    """
    As a resident
    I want to be able to cancel reservations on an order multiple times
    So that I can get some of my money back
    """)]
public partial class ResidentOrderMultipleCancellations
{
    [Scenario]
    public Task ResidentCancelsOneOfMultipleReservations() =>
        Runner.RunScenarioAsync(
            GivenAResidentIsSignedIn,
            GivenAnOrderIsPlacedAndPaid,
            WhenAReservationsIsCancelled,
            WhenTwoMoreReservationsAreCancelled,
            ThenTheFirstCancellationHasAFee,
            ThenTheSecondCancellationHasAFee,
            ThenTheCancellationFeesAreTheSame,
            ThenTheResidentsBalanceIsThePriceOfTheCancelledReservationsMinusTheCancellationFees,
            ThenTwoCancellationsHaveBeenAudited);
}
