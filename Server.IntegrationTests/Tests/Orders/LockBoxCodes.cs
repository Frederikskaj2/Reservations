using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

[FeatureDescription(
    """
    As a resident
    I want to receive an email with lock box codes before my reservation starts
    So that I'm able to enter the room
    """)]
public partial class LockBoxCodes
{
    [Scenario]
    public Task OrderWithOneReservationSendsEmail() =>
        Runner.RunScenarioAsync(
            _ => GivenAResidentHasPlacedAndPaidAnOrder(1),
            _ => GivenTheFirstReservationWillStartInAFewDays(),
            _ => WhenTheJobToSendLockBoxCodesExecute(),
            _ => ThenTheUserReceivesAnEmailWithLockBoxCodes());

    [Scenario]
    public Task OrderWithOneReservationSendsOnlyOneEmail() =>
        Runner.RunScenarioAsync(
            _ => GivenAResidentHasPlacedAndPaidAnOrder(1),
            _ => GivenTheFirstReservationWillStartInAFewDays(),
            _ => WhenTheJobToSendLockBoxCodesExecutesAgain(),
            _ => ThenTheUserDoesNotReceiveAnEmailWithLockBoxCodes());

    [Scenario]
    public Task OrderWithTwoReservationsSendsEmail() =>
        Runner.RunScenarioAsync(
            _ => GivenAResidentHasPlacedAndPaidAnOrder(2),
            _ => GivenTheFirstReservationWillStartInAFewDays(),
            _ => WhenTheJobToSendLockBoxCodesExecute(),
            _ => ThenTheUserReceivesAnEmailWithLockBoxCodes());

    [Scenario]
    public Task OrderWithTwoReservationsSendsOnlyOneEmail() =>
        Runner.RunScenarioAsync(
            _ => GivenAResidentHasPlacedAndPaidAnOrder(2),
            _ => GivenTheFirstReservationWillStartInAFewDays(),
            _ => WhenTheJobToSendLockBoxCodesExecutesAgain(),
            _ => ThenTheUserDoesNotReceiveAnEmailWithLockBoxCodes());
}
