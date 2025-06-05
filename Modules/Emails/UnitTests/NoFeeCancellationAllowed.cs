using FluentAssertions;
using NodaTime;
using System.Threading.Tasks;
using Xunit;
using static Frederikskaj2.Reservations.Emails.UnitTests.HtmlEntityEncoder;

namespace Frederikskaj2.Reservations.Emails.UnitTests;

public class NoFeeCancellationAllowed(MessageFactoryFixture fixture) : IClassFixture<MessageFactoryFixture>
{
    [Fact]
    public async Task FiveDaysDurationTest()
    {
        var orderId = Generate.OrderId();
        var model = new NoFeeCancellationAllowedDto(orderId, Duration.FromDays(5));

        var message = await fixture.CreateMessage(model);

        message.From.Should().Be(fixture.FromEmailAddress.ToString());
        message.ReplyTo.Should().Be(fixture.ReplyToEmailAddress.ToString());
        message.To.Should().BeEquivalentTo(fixture.ToEmailAddress.ToString());
        message.Subject.Should().Be($"Bestilling {orderId}");
        message.Body.Should().Be(
            $"""
             <!DOCTYPE html>
             <html lang="da">
             <head>
                 <title>Bestilling {orderId}</title>
                 <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
                 <meta http-equiv="X-UA-Compatible" content="IE=edge">
                 <meta name="viewport" content="width=device-width, initial-scale=1.0">
             </head>
             <body>
                 <p>Hej {HtmlEncode(fixture.ToFullName)}</p>
                 <p>Du har nu mulighed for at afbestille reservationer på <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/bestillinger/{orderId}">bestillingsnr.&#xA0;{orderId}</a> uden at betale afbestillingsgebyr.</p>
                 <p>Du kan gøre dette de n&#xE6;ste 5 dage.</p>
                 <p><em>(Dette er en automatisk udsendt besked som ikke skal besvares.)</em></p>
                 <p>Med venlig hilsen<br>{HtmlEncode(fixture.FromName)}<br><a href="{HtmlEncode(fixture.FromUrl.ToString())}">{HtmlEncode(fixture.FromUrl.ToString())}</a></p>
                 <p><a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/bestillinger/{orderId}">Din bestilling</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/kontoudtog">Dit kontoudtog</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/konto">Din konto</a></p>
             </body>
             </html>
             """);
    }

    [Fact]
    public async Task OneDaysDurationTest()
    {
        var orderId = Generate.OrderId();
        var model = new NoFeeCancellationAllowedDto(orderId, Duration.FromDays(1));

        var message = await fixture.CreateMessage(model);

        message.From.Should().Be(fixture.FromEmailAddress.ToString());
        message.ReplyTo.Should().Be(fixture.ReplyToEmailAddress.ToString());
        message.To.Should().BeEquivalentTo(fixture.ToEmailAddress.ToString());
        message.Subject.Should().Be($"Bestilling {orderId}");
        message.Body.Should().Be(
            $"""
             <!DOCTYPE html>
             <html lang="da">
             <head>
                 <title>Bestilling {orderId}</title>
                 <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
                 <meta http-equiv="X-UA-Compatible" content="IE=edge">
                 <meta name="viewport" content="width=device-width, initial-scale=1.0">
             </head>
             <body>
                 <p>Hej {HtmlEncode(fixture.ToFullName)}</p>
                 <p>Du har nu mulighed for at afbestille reservationer på <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/bestillinger/{orderId}">bestillingsnr.&#xA0;{orderId}</a> uden at betale afbestillingsgebyr.</p>
                 <p>Du kan gøre dette den n&#xE6;ste dag.</p>
                 <p><em>(Dette er en automatisk udsendt besked som ikke skal besvares.)</em></p>
                 <p>Med venlig hilsen<br>{HtmlEncode(fixture.FromName)}<br><a href="{HtmlEncode(fixture.FromUrl.ToString())}">{HtmlEncode(fixture.FromUrl.ToString())}</a></p>
                 <p><a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/bestillinger/{orderId}">Din bestilling</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/kontoudtog">Dit kontoudtog</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/konto">Din konto</a></p>
             </body>
             </html>
             """);
    }
}
