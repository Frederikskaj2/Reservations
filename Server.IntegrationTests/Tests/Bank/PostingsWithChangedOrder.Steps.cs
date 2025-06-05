using FluentAssertions;
using Frederikskaj2.Reservations.Bank;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Users;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Framework;
using LightBDD.XUnit2;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Server.IntegrationTests.Harness.AmountFunctions;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Bank;

sealed partial class PostingsWithChangedOrder(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    State<MyOrderDto> order1;
    State<ReservationDto> reservationToCancel;
    State<Amount> fee;
    State<MyOrderDto> order2;
    State<IEnumerable<PostingDto>> postings;
    State<IEnumerable<ResidentDto>> residents;
    State<IEnumerable<CreditorDto>> creditors;

    MyOrderDto Order1 => order1.GetValue(nameof(Order1));
    ReservationDto ReservationToCancel => reservationToCancel.GetValue(nameof(ReservationToCancel));
    Amount Fee => fee.GetValue(nameof(Fee));
    MyOrderDto Order2 => order2.GetValue(nameof(Order2));
    IEnumerable<PostingDto> Postings => postings.GetValue(nameof(Postings));
    IEnumerable<ResidentDto> Residents => residents.GetValue(nameof(Residents));
    IEnumerable<CreditorDto> Creditors => creditors.GetValue(nameof(Creditors));

    async Task IScenarioSetUp.OnScenarioSetUp() => await session.UpdateLockBoxCodes();

    async Task GivenAResident() => await session.SignUpAndSignIn();

    async Task GivenAPaidOrder()
    {
        var getMyOrderResponse = await session.PlaceAndPayResidentOrder(
            new TestReservation(SeedData.BanquetFacilities.ResourceId, 2),
            new TestReservation(SeedData.Frederik.ResourceId));
        order1 = getMyOrderResponse.Order;
        reservationToCancel = getMyOrderResponse.Order.Reservations.First();
        await session.ConfirmOrders();
    }

    async Task GivenThePaidOrderIsCancelled()
    {
        var updateResidentOrderResponse = await session.CancelReservation(Order1.OrderId, 0);
        fee = -updateResidentOrderResponse.Order.Resident!.AdditionalLineItems.Single().Amount;
    }

    async Task GivenTheOrderIsPlacedAgain()
    {
        var getMyOrderResponse = await session.PlaceResidentOrder(new ReservationRequest(ReservationToCancel.ResourceId, ReservationToCancel.Extent));
        order2 = getMyOrderResponse.Order;
    }

    async Task WhenThePostingsAreRetrieved()
    {
        var getPostingsResponse = await session.GetPostings(session.CurrentDate);
        postings = new(getPostingsResponse.Postings);
    }

    async Task WhenTheResidentsAreRetrieved()
    {
        var getResidentsResponse = await session.GetResidents();
        residents = new(getResidentsResponse.Residents);
    }

    async Task WhenTheCreditorsAreRetrieved()
    {
        var getCreditorsResponse = await session.GetCreditors();
        creditors = new(getCreditorsResponse.Creditors);
    }

    Task ThenTheResidentPaidAFee()
    {
        Fee.Should().BeGreaterThan(Amount.Zero);
        return Task.CompletedTask;
    }

    Task ThenFourPostingsAreReturned()
    {
        Postings.Should().HaveCount(4);
        return Task.CompletedTask;
    }

    Task ThenTheFirstPostingIsForPlacingTheOrder()
    {
        var posting = Postings.First();
        posting.Should().BeEquivalentTo(
            new
            {
                Date = session.CurrentDate,
                Activity = Activity.PlaceOrder,
                session.User!.FullName,
                Order1.OrderId,
                Amounts = new[]
                {
                    new { Account = PostingAccount.Income, Amount = Credit(Order1.Price.Total() - Order1.Price.Deposit) },
                    new { Account = PostingAccount.Deposits, Amount = Credit(Order1.Price.Deposit) },
                    new { Account = PostingAccount.AccountsReceivable, Amount = Debit(Order1.Price.Total()) },
                },
            });
        return Task.CompletedTask;
    }

    Task ThenTheSecondPostingIsForPayingTheOrder()
    {
        var posting = Postings.ElementAt(1);
        posting.Should().BeEquivalentTo(
            new
            {
                Date = session.CurrentDate,
                Activity = Activity.PayIn,
                session.User!.FullName,
                OrderId = (OrderId?) null,
                Amounts = new[]
                {
                    new { Account = PostingAccount.AccountsReceivable, Amount = Credit(Order1.Price.Total()) },
                    new { Account = PostingAccount.Bank, Amount = Debit(Order1.Price.Total()) },
                },
            });
        return Task.CompletedTask;
    }

    Task ThenTheThirdPostingIsForCancellingTheOrder()
    {
        var posting = Postings.ElementAt(2);
        posting.Should().BeEquivalentTo(
            new
            {
                Date = session.CurrentDate,
                Activity = Activity.UpdateOrder,
                session.User!.FullName,
                Order1.OrderId,
                Amounts = new[]
                {
                    new { Account = PostingAccount.Income, Amount = Debit(ReservationToCancel.Price!.Total() - ReservationToCancel.Price.Deposit - Fee) },
                    new { Account = PostingAccount.Deposits, Amount = Debit(ReservationToCancel.Price.Deposit) },
                    new { Account = PostingAccount.AccountsPayable, Amount = Credit(ReservationToCancel.Price.Total() - Fee) },
                },
            });
        return Task.CompletedTask;
    }

    Task ThenTheFourthPostingIsForPlacingTheOrderAgain()
    {
        var posting = Postings.Last();
        posting.Should().BeEquivalentTo(
            new
            {
                Date = session.CurrentDate,
                Activity = Activity.PlaceOrder,
                session.User!.FullName,
                Order2.OrderId,
                Amounts = new[]
                {
                    new { Account = PostingAccount.Income, Amount = Credit(Order2.Price.Total() - Order2.Price.Deposit) },
                    new { Account = PostingAccount.Deposits, Amount = Credit(Order2.Price.Deposit) },
                    new { Account = PostingAccount.AccountsReceivable, Amount = Debit(Fee) },
                    new { Account = PostingAccount.AccountsPayable, Amount = Debit(ReservationToCancel.Price!.Total() - Fee) },
                },
            });
        return Task.CompletedTask;
    }

    Task ThenThereIsOneResidentWithDebt()
    {
        Residents.Should().ContainSingle();
        Residents.Single().Balance.Should().Be(-Fee);
        return Task.CompletedTask;
    }

    Task ThenThereAreNoCreditors()
    {
        Creditors.Should().BeEmpty();
        return Task.CompletedTask;
    }
}
