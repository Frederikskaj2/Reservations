using FluentAssertions;
using Frederikskaj2.Reservations.LockBox;
using System.Threading.Tasks;
using Xunit;
using static Frederikskaj2.Reservations.Emails.UnitTests.HtmlEntityEncoder;

namespace Frederikskaj2.Reservations.Emails.UnitTests;

public class SettlementNeeded(MessageFactoryFixture fixture) : IClassFixture<MessageFactoryFixture>
{
    [Fact]
    public async Task Test()
    {
        var orderId = Generate.OrderId();
        var resourceId = Generate.ResourceId();
        var date = Generate.Date();
        var model = new SettlementNeededDto(orderId, resourceId, date);

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
                 <p>Brug af {HtmlEncode(Resources.GetNameUnsafe(resourceId))} på <a href="{HtmlEncode(fixture.FromUrl.ToString())}bestillinger/{orderId}">bestillingsnr.&#xA0;{orderId}</a> sluttede {date.ToDateOnly().ToString("d. MMMM yyyy", fixture.CultureInfo)}, og der mangler nu en opgørelse.</p>
                 <p><em>(Dette er en automatisk udsendt besked som ikke skal besvares.)</em></p>
                 <p>Med venlig hilsen<br>{HtmlEncode(fixture.FromName)}<br><a href="{HtmlEncode(fixture.FromUrl.ToString())}">{HtmlEncode(fixture.FromUrl.ToString())}</a></p>
                 <p><a href="{HtmlEncode(fixture.FromUrl.ToString())}bestillinger">Bestillinger</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/konto">Din konto</a></p>
             </body>
             </html>
             """);
    }
}
