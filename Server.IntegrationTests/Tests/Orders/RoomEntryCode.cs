using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

[FeatureDescription(
    """
    As a resident
    I want to receive an email with an entry code before my reservation starts
    So that I'm able to enter the room
    """)]
public partial class RoomEntryCode
{
    [Scenario]
    public Task OrderWithOneReservationSendsEmail() =>
        Runner.RunScenarioAsync(
            _ => GivenAResidentHasPlacedAndPaidAnOrder(1),
            _ => GivenTheFirstReservationWillStartInAFewDays(),
            _ => WhenTheJobToSendRoomEntryCodesExecute(),
            _ => ThenTheUserReceivesAnEmailWithAnEntryCode());

    [Scenario]
    public Task OrderWithOneReservationSendsOnlyOneEmail() =>
        Runner.RunScenarioAsync(
            _ => GivenAResidentHasPlacedAndPaidAnOrder(1),
            _ => GivenTheFirstReservationWillStartInAFewDays(),
            _ => WhenTheJobToSendRoomEntryCodesExecutesAgain(),
            _ => ThenTheUserDoesNotReceiveAnEmailWithAnEntryCode());

    [Scenario]
    public Task OrderWithTwoReservationsSendsEmail() =>
        Runner.RunScenarioAsync(
            _ => GivenAResidentHasPlacedAndPaidAnOrder(2),
            _ => GivenTheFirstReservationWillStartInAFewDays(),
            _ => WhenTheJobToSendRoomEntryCodesExecute(),
            _ => ThenTheUserReceivesAnEmailWithAnEntryCode());

    [Scenario]
    public Task OrderWithTwoReservationsSendsOnlyOneEmail() =>
        Runner.RunScenarioAsync(
            _ => GivenAResidentHasPlacedAndPaidAnOrder(2),
            _ => GivenTheFirstReservationWillStartInAFewDays(),
            _ => WhenTheJobToSendRoomEntryCodesExecutesAgain(),
            _ => ThenTheUserDoesNotReceiveAnEmailWithAnEntryCode());
}
