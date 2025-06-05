using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Users;

[FeatureDescription(
    """
    As a user
    I want to delete my account
    So that I am removed from the system
    """)]
public partial class DeleteUser
{
    [Scenario]
    public Task UserDeletesTheirAccount() =>
        Runner.RunScenarioAsync(
            GivenAUserIsSignedIn,
            WhenTheUserRequestsDeletion,
            WhenTheDeleteUsersJobHasExecuted,
            ThenTheDeletionIsSuccessful,
            ThenTheUserReceivesAnEmailAboutTheDeletion,
            ThenTheUsersPersonalInformationIsNoLongerAvailable,
            ThenTheUserIsDeleted);

    [Scenario]
    public Task ResidentWithAnUnpaidOrderDeletesTheirAccount() =>
        Runner.RunScenarioAsync(
            GivenAUserIsSignedIn,
            GivenTheUserHasAnUnpaidOrder,
            WhenTheUserRequestsDeletion,
            WhenTheDeleteUsersJobHasExecuted,
            ThenTheDeletionIsPending,
            ThenTheUsersPersonalInformationIsAvailable,
            ThenTheUserIsPendingDeletion);

    [Scenario]
    public Task ResidentWithPendingDeleteCancelsOrder() =>
        Runner.RunScenarioAsync(
            GivenAUserIsSignedIn,
            GivenTheUserHasAnUnpaidOrder,
            WhenTheUserRequestsDeletion,
            WhenTheOrderIsCancelledByTheUser,
            WhenTheDeleteUsersJobHasExecuted,
            ThenTheResponseContainsInformationAboutTheDeletion,
            ThenTheUserReceivesAnEmailAboutTheDeletion,
            ThenTheUsersPersonalInformationIsNoLongerAvailable,
            ThenTheUserIsDeleted);

    [Scenario]
    public Task ResidentWithPendingDeleteHasOrderCancelledByAnAdministrator() =>
        Runner.RunScenarioAsync(
            GivenAUserIsSignedIn,
            GivenTheUserHasAnUnpaidOrder,
            WhenTheUserRequestsDeletion,
            WhenTheOrderIsCancelledByAnAdministrator,
            WhenTheDeleteUsersJobHasExecuted,
            ThenTheUserReceivesAnEmailAboutTheDeletion,
            ThenTheUsersPersonalInformationIsNoLongerAvailable,
            ThenTheUserIsDeleted);

    [Scenario]
    public Task ResidentWithPendingDeleteHasOrderSettledByAnAdministrator() =>
        Runner.RunScenarioAsync(
            GivenAUserIsSignedIn,
            GivenTheUserHasAPaidOrder,
            WhenTheUserRequestsDeletion,
            WhenTheOrderIsSettledWithDamagesEqualToDeposit,
            WhenTheDeleteUsersJobHasExecuted,
            ThenTheUserReceivesAnEmailAboutTheDeletion,
            ThenTheUsersPersonalInformationIsNoLongerAvailable,
            ThenTheUserIsDeleted);

    [Scenario]
    public Task ResidentWithPendingDeleteHasRemainingBalanceRefunded() =>
        Runner.RunScenarioAsync(
            GivenAUserIsSignedIn,
            GivenTheUserHasAPaidOrder,
            WhenTheOrderIsSettled,
            WhenTheUserRequestsDeletion,
            WhenTheResidentsBalanceIsRefunded,
            WhenTheDeleteUsersJobHasExecuted,
            ThenTheUserReceivesAnEmailAboutTheDeletion,
            ThenTheUsersPersonalInformationIsNoLongerAvailable,
            ThenTheUserIsDeleted);
}
