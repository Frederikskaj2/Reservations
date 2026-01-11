using Frederikskaj2.Reservations.Bank;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using LightBDD.Framework;
using LightBDD.XUnit2;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Bank;

public abstract class PayOutsFixture(SessionFixture session) : FeatureFixture, IClassFixture<SessionFixture>
{
    protected SessionFixture Session { get; } = session;

    State<CreditorDto> creditor;
    State<PayOutSummaryDto> payout;
    State<IEnumerable<PayOutSummaryDto>> retrievedPayOuts;
    State<PayOutDetailsDto> retrievedPayOut;

    protected CreditorDto Creditor => creditor.GetValue(nameof(Creditor));
    protected PayOutSummaryDto PayOut => payout.GetValue(nameof(PayOut));
    protected IEnumerable<PayOutSummaryDto> RetrievedPayOuts => retrievedPayOuts.GetValue(nameof(RetrievedPayOuts));
    protected PayOutDetailsDto RetrievedPayOut => retrievedPayOut.GetValue(nameof(RetrievedPayOut));

    protected async Task GivenASettledOrder()
    {
        await Session.SignUpAndSignIn();
        var getMyOrderResponse = await Session.PlaceAndPayResidentOrder(new TestReservation(SeedData.Frederik.ResourceId));
        await Session.RunConfirmOrders();
        await Session.SettleReservation(getMyOrderResponse.Order.OrderId, 0);
        var getCreditorsResponse = await Session.GetCreditors();
        creditor = getCreditorsResponse.Creditors.Single(c => c.UserInformation.UserId == Session.UserId());
    }

    protected async Task WhenAPayOutIsCreated()
    {
        var createPayOutResponse = await Session.CreatePayOut(Creditor.UserInformation.UserId, Creditor.Payment.AccountNumber, Creditor.Payment.Amount);
        payout = createPayOutResponse.PayOut;
    }

    protected async Task WhenPayOutsAreRetrieved()
    {
        var getPayOutsResponse = await Session.GetPayOuts();
        retrievedPayOuts = new(getPayOutsResponse.PayOuts);
    }

    protected async Task WhenThePayOutIsRetrieved()
    {
        var getPayOutResponse = await Session.GetPayOut(PayOut.PayOutId);
        retrievedPayOut = new(getPayOutResponse.PayOut);
    }
}
