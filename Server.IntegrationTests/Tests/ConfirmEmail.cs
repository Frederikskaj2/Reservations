using FluentAssertions;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests;

public class ConfirmEmail : IClassFixture<SessionFixture>
{
    public ConfirmEmail(SessionFixture session) => Session = session;

    SessionFixture Session { get; }

    [Fact]
    public async Task SignUp()
    {
        await Session.SignUpAsync();
        await Session.SignInAsync();
        var user = await Session.GetUserAsync(Session.UserId());
        user.IsEmailConfirmed.Should().BeFalse();
    }

    [Fact]
    public async Task SignUpAndConfirmEmail()
    {
        await Session.SignUpAsync();
        await Session.ConfirmEmailAsync();
        await Session.SignInAsync();
        var user = await Session.GetUserAsync(Session.UserId());
        user.IsEmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task SignUpAndConfirmEmailAndSignUpAgain()
    {
        await Session.SignUpAsync();
        await Session.ConfirmEmailAsync();
        await Session.SignUpAgainAsync();
        await Session.SignInAsync();
        var user = await Session.GetUserAsync(Session.UserId());
        user.IsEmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task SignUpTwiceAndConfirmEmail()
    {
        await Session.SignUpAsync();
        await Session.SignUpAgainAsync();
        await Session.ConfirmEmailAsync();
        await Session.SignInAsync();
        var user = await Session.GetUserAsync(Session.UserId());
        user.IsEmailConfirmed.Should().BeTrue();
    }
}
