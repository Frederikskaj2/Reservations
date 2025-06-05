using FluentAssertions;
using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Users;
using LightBDD.Framework;
using LightBDD.XUnit2;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Users;

partial class UpdateUser(SessionFixture session) : FeatureFixture, IClassFixture<SessionFixture>
{
    State<string> phone;
    State<HttpResponseMessage> response;

    string Phone => phone.GetValue(nameof(Phone));
    HttpResponseMessage Response => response.GetValue(nameof(Response));

    async Task GivenASignedInUser() => await session.SignUpAndSignIn();

    async Task GivenASignedInResident() => await session.SignUpAndSignIn();

    async Task GivenTheUserIsBothAResidentAndAnAdministrator() =>
        await session.UpdateUser(session.UserId(), session.User!.FullName, session.User.Phone, Roles.Resident | Roles.OrderHandling);

    async Task GivenTheUserIsAnAdministrator() =>
        await session.UpdateUser(session.UserId(), session.User!.FullName, session.User.Phone, Roles.OrderHandling);

    async Task WhenTheUserUpdatesTheirName() => await session.UpdateMyUser(Generate.FullName(), session.User!.Phone);

    async Task WhenTheUserUpdatesTheirPhoneNumber()
    {
        phone = Generate.Phone();
        await session.UpdateMyUser(session.User!.FullName, Phone);
    }

    async Task WhenTheUserUpdatesTheirEmailSubscriptions() =>
        response = await session.UpdateMyEmailSubscriptionsRaw(EmailSubscriptions.NewOrder);

    async Task ThenTheNameOfTheUserIsUpdated()
    {
        var myUserResponse = await session.GetMyUser();
        myUserResponse.Identity.FullName.Should().Be(session.User!.FullName);
    }

    async Task ThenTheChangeOfTheNameIsAudited()
    {
        var getUserResponse = await session.GetUser(session.UserId());
        getUserResponse.User.Audits
            .Should().ContainSingle(audit => audit.Type == UserAuditType.UpdateFullName).Which
            .Should().BeEquivalentTo(new { session.User!.FullName, UserId = session.UserId() });
    }

    async Task ThenThePhoneNumberOfTheUserIsUpdated()
    {
        var myUserResponse = await session.GetMyUser();
        myUserResponse.Identity.Phone.Replace(" ", "", StringComparison.Ordinal).Should().Be(Phone);
    }

    async Task ThenTheChangeOfThePhoneNumberIsAudited()
    {
        var getUserResponse = await session.GetUser(session.UserId());
        getUserResponse.User.Audits
            .Should().ContainSingle(audit => audit.Type == UserAuditType.UpdatePhone).Which
            .Should().BeEquivalentTo(new { session.User!.FullName, UserId = session.UserId() });
    }

    Task ThenTheRequestFails()
    {
        Response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        return Task.CompletedTask;
    }

    Task ThenTheRequestSucceeds()
    {
        Response.StatusCode.Should().Be(HttpStatusCode.OK);
        return Task.CompletedTask;
    }
}
