using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Users;

[FeatureDescription(
    """
    As an administrator
    I want to be able to retrieve and update a user's information
    So that I can ensure the user's details are correctly registered in the system
    """)]
public partial class UserAdministration
{
    [Scenario]
    public Task AdministratorUpdatesTheNameOfAUser() =>
        Runner.RunScenarioAsync(
            GivenAUser,
            WhenAnAdministratorUpdatesTheNameOfTheUser,
            ThenTheNameOfTheUserIsUpdated,
            ThenTheChangeOfTheNameIsAudited);

    [Scenario]
    public Task AdministratorUpdatesThePhoneNumberOfAUser() =>
        Runner.RunScenarioAsync(
            GivenAUser,
            WhenAnAdministratorUpdatesThePhoneNumberOfTheUser,
            ThenThePhoneNumberOfTheUserIsUpdated,
            ThenTheChangeOfThePhoneNumberIsAudited);

    [Scenario]
    public Task AdministratorGetsTheTimestampOfTheLatestSignInOfAUser() =>
        Runner.RunScenarioAsync(
            GivenAUserSignsIn,
            WhenAnAdministratorGetsInformationAboutTheUser,
            ThenTheTimestampOfTheLatestSignInOfTheUserIsAvailable);
}
