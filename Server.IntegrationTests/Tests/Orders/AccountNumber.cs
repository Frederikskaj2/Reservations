using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

[FeatureDescription(
    """
    As a resident
    I want my account number to be updated
    So that I can control where my deposit is refunded to
    """)]
public partial class AccountNumber
{
    [Scenario]
    public Task ResidentUpdatesAccountNumber() =>
        Runner.RunScenarioAsync(
            GivenAResidentHasPlacedAnOrder,
            WhenTheResidentUpdatesTheAccountNumber,
            ThenTheOrderHasTheNewAccountNumber,
            ThenTheResidentCanSeeTheUpdatedBankAccountNumber,
            ThenTheResidentsChangeOfAccountNumberIsAudited);

    [Scenario]
    public Task AdministratorUpdatesAccountNumber() =>
        Runner.RunScenarioAsync(
            GivenAResidentHasPlacedAnOrder,
            WhenTheAdministratorUpdatesTheAccountNumber,
            ThenTheResidentCanSeeTheUpdatedBankAccountNumber,
            ThenTheAdministratorsChangeOfAccountNumberIsAudited);
}
