using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests;

public class Password : IClassFixture<SessionFixture>
{
    public Password(SessionFixture session) => Session = session;

    SessionFixture Session { get; }

    [Fact]
    public async Task UpdatePassword()
    {
        await Session.SignUpAsync();
        await Session.SignInAsync();
        await Session.UpdatePassword();
        await Session.SignInAsync();
    }
}
