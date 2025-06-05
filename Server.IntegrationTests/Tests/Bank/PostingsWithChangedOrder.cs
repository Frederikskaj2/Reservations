using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Bank;

[FeatureDescription(
    """
    As an accountant
    I want to view a general journal of transactions
    So that I can perform accounting using double-entry bookkeeping
    """)]
public partial class PostingsWithChangedOrder
{
    [Scenario]
    public Task GetPostings() =>
        Runner.RunScenarioAsync(
            GivenAResident,
            GivenAPaidOrder,
            GivenThePaidOrderIsCancelled,
            GivenTheOrderIsPlacedAgain,
            WhenThePostingsAreRetrieved,
            WhenTheResidentsAreRetrieved,
            WhenTheCreditorsAreRetrieved,
            ThenTheResidentPaidAFee,
            ThenFourPostingsAreReturned,
            ThenTheFirstPostingIsForPlacingTheOrder,
            ThenTheSecondPostingIsForPayingTheOrder,
            ThenTheThirdPostingIsForCancellingTheOrder,
            ThenTheFourthPostingIsForPlacingTheOrderAgain,
            ThenThereIsOneResidentWithDebt,
            ThenThereAreNoCreditors);
}
