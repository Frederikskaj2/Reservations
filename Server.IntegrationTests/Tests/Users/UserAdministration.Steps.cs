using FluentAssertions;
using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Users;
using LightBDD.Framework;
using LightBDD.XUnit2;
using NodaTime;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Users;

partial class UserAdministration(SessionFixture session) : FeatureFixture, IClassFixture<SessionFixture>
{
    State<Instant> begin;
    State<Instant> end;
    State<string> fullName;
    State<GetUserResponse> getUserResponse;
    State<string> phone;

    string FullName => fullName.GetValue(nameof(FullName));
    string Phone => phone.GetValue(nameof(Phone));
    Instant Begin => begin.GetValue(nameof(Begin));
    GetUserResponse GetUserResponse => getUserResponse.GetValue(nameof(GetUserResponse));
    Instant End => end.GetValue(nameof(End));

    async Task GivenAUser() => await session.SignUpAndSignIn();

    async Task GivenAUserSignsIn()
    {
        begin = SystemClock.Instance.GetCurrentInstant();
        await session.SignUpAndSignIn();
    }

    async Task WhenAnAdministratorUpdatesTheNameOfTheUser()
    {
        fullName = Generate.FullName();
        await session.UpdateUser(session.UserId(), FullName, session.User!.Phone, Roles.Resident);
    }

    async Task WhenAnAdministratorUpdatesThePhoneNumberOfTheUser()
    {
        phone = Generate.Phone();
        await session.UpdateUser(session.UserId(), session.User!.FullName, Phone, Roles.Resident);
    }

    async Task WhenAnAdministratorGetsInformationAboutTheUser()
    {
        getUserResponse = await session.GetUser(session.UserId());
        end = SystemClock.Instance.GetCurrentInstant();
    }

    async Task ThenTheNameOfTheUserIsUpdated()
    {
        var myUserResponse = await session.GetMyUser();
        myUserResponse.Identity.FullName.Should().Be(FullName);
    }

    async Task ThenTheChangeOfTheNameIsAudited()
    {
        var response = await session.GetUser(session.UserId());
        response.User.Audits
            .Should().ContainSingle(audit => audit.Type == UserAuditType.UpdateFullName).Which
            .Should().BeEquivalentTo(new { UserId = SeedData.AdministratorUserId });
    }

    async Task ThenThePhoneNumberOfTheUserIsUpdated()
    {
        var myUserResponse = await session.GetMyUser();
        myUserResponse.Identity.Phone.Replace(" ", "", StringComparison.Ordinal).Should().Be(Phone);
    }

    async Task ThenTheChangeOfThePhoneNumberIsAudited()
    {
        var response = await session.GetUser(session.UserId());
        response.User.Audits
            .Should().ContainSingle(audit => audit.Type == UserAuditType.UpdatePhone).Which
            .Should().BeEquivalentTo(new { UserId = SeedData.AdministratorUserId });
    }

    Task ThenTheTimestampOfTheLatestSignInOfTheUserIsAvailable()
    {
        GetUserResponse.User.LatestSignIn.Should().NotBeNull();
        GetUserResponse.User.LatestSignIn!.Value.Should().BeGreaterThanOrEqualTo(Begin);
        GetUserResponse.User.LatestSignIn!.Value.Should().BeLessThanOrEqualTo(End);
        return Task.CompletedTask;
    }
}
