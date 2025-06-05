using FluentAssertions;
using Frederikskaj2.Reservations.Bank;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Framework;
using LightBDD.XUnit2;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Bank;

sealed partial class PayOuts(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    State<CreditorDto> creditor;
    State<PayOutDto> payout;
    State<string?> eTag;
    State<HttpStatusCode> statusCode;
    State<IEnumerable<PayOutDto>> retrievedPayOuts;

    CreditorDto Creditor => creditor.GetValue(nameof(Creditor));
    PayOutDto PayOut => payout.GetValue(nameof(PayOut));
    string? ETag => eTag.GetValue(nameof(ETag));
    HttpStatusCode StatusCode => statusCode.GetValue(nameof(StatusCode));
    IEnumerable<PayOutDto> RetrievedPayOuts => retrievedPayOuts.GetValue(nameof(RetrievedPayOuts));

    async Task IScenarioSetUp.OnScenarioSetUp() => await session.UpdateLockBoxCodes();

    async Task GivenASettledOrder()
    {
        await session.SignUpAndSignIn();
        var getMyOrderResponse = await session.PlaceAndPayResidentOrder(new TestReservation(SeedData.Frederik.ResourceId));
        await session.ConfirmOrders();
        await session.SettleReservation(getMyOrderResponse.Order.OrderId, 0);
        var getCreditorsResponse = await session.GetCreditors();
        creditor = getCreditorsResponse.Creditors.Single(c => c.UserInformation.UserId == session.UserId());
    }

    async Task WhenAPayOutIsCreated()
    {
        var clientCreatePayOutResponse = await session.CreatePayOut(Creditor.UserInformation.UserId, Creditor.Payment.Amount);
        payout = clientCreatePayOutResponse.PayOut;
        eTag = clientCreatePayOutResponse.ETag;
    }

    async Task WhenThePayOutIsDeleted() => await session.DeletePayOut(PayOut.PayOutId, ETag);

    async Task WhenThePayOutIsDeletedWithInvalidETag()
    {
        var response = await session.DeletePayOutRaw(
            PayOut.PayOutId,
            """
            "Invalid ETag"
            """);
        statusCode = response.StatusCode;
    }

    async Task WhenPayOutsAreRetrieved()
    {
        var getPayOutsResponse = await session.GetPayOuts();
        retrievedPayOuts = new(getPayOutsResponse.PayOuts);
    }

    Task ThenThePayOutIsCreated()
    {
        PayOut.UserIdentity.Should().BeEquivalentTo(Creditor.UserInformation);
        PayOut.PaymentId.Should().BeEquivalentTo(Creditor.Payment.PaymentId);
        PayOut.Amount.Should().BeEquivalentTo(Creditor.Payment.Amount);
        return Task.CompletedTask;
    }

    Task ThenTheRetrievedPayOutsContainThePayout()
    {
        RetrievedPayOuts.Should().ContainEquivalentOf(new { PayOut.PayOutId });
        return Task.CompletedTask;
    }

    Task ThenTheRetrievedPayOutsDoesNotContainThePayout()
    {
        RetrievedPayOuts.Should().NotContainEquivalentOf(new { PayOut.PayOutId });
        return Task.CompletedTask;
    }

    Task ThenTheHttpStatusIsPreconditionFailed()
    {
        StatusCode.Should().Be(HttpStatusCode.PreconditionFailed);
        return Task.CompletedTask;
    }
}
