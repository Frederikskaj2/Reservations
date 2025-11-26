using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

[FeatureDescription(
    """
    As an administrator
    I want to be able to reimburse a resident to cover unexpected negative circumstances related to their reservation
    So that an additional amount can be refunded to the resident on top of their deposit
    """)]
public partial class Reimburse
{
    [Scenario]
    public Task ReimburseCleaning() =>
        Runner.RunScenarioAsync(
            GivenAResidentIsSignedIn,
            GivenAPaidOrder,
            GivenTheOrderIsSettled,
            WhenTheCleaningIsReimbursed,
            WhenThePostingsAreRetrieved,
            WhenTheResidentsTransactionsAreRetrieved,
            ThenTheResidentIsOwedTheDepositPlusTheReimbursedAmountForLackOfCleaning,
            ThenTheResidentsBalanceIsTheDepositPlusTheReimbursedAmountForLackOfCleaning,
            ThenTheResidentsLastTransactionIsDescribedAsCleaning,
            ThenFourPostingsAreReturned,
            ThenTheLastPostingIsTheReimbursementUsingAccountsPayable);

    [Scenario]
    public Task ReimburseCleaningWithNegativeBalance() =>
        Runner.RunScenarioAsync(
            GivenAResidentIsSignedIn,
            GivenAPaidOrder,
            GivenTheOrderIsSettled,
            GivenAnUnpaidOrder,
            WhenTheCleaningIsReimbursed,
            WhenThePostingsAreRetrieved,
            WhenTheResidentsTransactionsAreRetrieved,
            ThenTheResidentIsNotOwedAnything,
            ThenTheResidentsBalanceIsTheDepositPlusTheReimbursedAmountForLackOfCleaningMinusTheUnpaidOrder,
            ThenTheResidentsLastTransactionIsDescribedAsCleaning,
            ThenFivePostingsAreReturned,
            ThenTheLastPostingIsTheReimbursementUsingAccountsReceivable);

}
