using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests;

public class SignUp : IClassFixture<SessionFixture>
{
    public SignUp(SessionFixture session) => Session = session;

    SessionFixture Session { get; }

    [Fact]
    public async Task SignUpTwiceWithoutNewPassword()
    {
        await Session.SignUpAsync();
        await Session.SignUpAgainAsync();
        await Session.SignInAsync();
    }

    [Fact]
    public async Task SignUpTwiceWithNewPassword()
    {
        await Session.SignUpAsync();
        await Session.SignUpAgainAsync();
        await Session.NewPasswordEmailAsync();
        await Session.SignInAsync();
    }
}
