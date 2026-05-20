using FluentAssertions;
using Frederikskaj2.Reservations.Orders;
using System.Threading.Tasks;
using Xunit;
using static Frederikskaj2.Reservations.Emails.UnitTests.HtmlEntityEncoder;

namespace Frederikskaj2.Reservations.Emails.UnitTests;

public class RoomEntryCode(MessageFactoryFixture fixture) : IClassFixture<MessageFactoryFixture>
{
    [Fact]
    public async Task Test()
    {
        var orderId = Generate.OrderId();
        var resourceId = Generate.ResourceId();
        var date = Generate.Date();
        var entryCode = Generate.EntryCode();
        var model = new RoomEntryCodeDto(orderId, resourceId, date, entryCode);

        var message = await fixture.CreateMessage(model);

        message.From.Should().Be(fixture.FromEmailAddress.ToString());
        message.ReplyTo.Should().Be(fixture.ReplyToEmailAddress.ToString());
        message.To.Should().BeEquivalentTo(fixture.ToEmailAddress.ToString());
        message.Subject.Should().Be($"Adgangskode til {Resources.GetNameUnsafe(resourceId)} den {date.ToDateOnly().ToString("d. MMMM yyyy", fixture.CultureInfo)}");
        var rulesPath = Resources.GetResourceType(resourceId) == ResourceType.BanquetFacilities ? "festlokale" : "sovevaerelser";
        message.Body.Should().Be(
            $"""
             <!DOCTYPE html>
             <html lang="da">
             <head>
                 <title>Adgangskode til {HtmlEncode(Resources.GetNameUnsafe(resourceId))} den {date.ToDateOnly().ToString("d. MMMM yyyy", fixture.CultureInfo)}</title>
                 <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
                 <meta http-equiv="X-UA-Compatible" content="IE=edge">
                 <meta name="viewport" content="width=device-width, initial-scale=1.0">
             </head>
             <body>
                 <p>Hej {HtmlEncode(fixture.ToFullName)}</p>
                 <p>Her er den adgangskode du skal bruge for at få adgang til <strong>{HtmlEncode(Resources.GetNameUnsafe(resourceId))}</strong> den {date.ToDateOnly().ToString("d. MMMM yyyy", fixture.CultureInfo)}:</p>
                 <p><strong>{entryCode}</strong></p>
                 <p>Låse op udenfor: Indtast adgangskoden på tastaturet og tryk på pilen.</p>
                 <p>Låse op indenfor: Drej låsen eller tryk på knappen i midten.</p>
                 <p>Låse af udenfor: Løft dørhåndtaget op og tryk dernæst på pilen på tastaturet.</p>
                 <p>Låse af indenfor: Løft dørhåndtaget op og drej dernæst låsen eller tryk på knappen i midten.</p>
                 <p>Der er WiFi i fælleshuset:</p>
                 <ul>
                     <li>Navn: <strong>Frederikskaj</strong></li>
                     <li>Kode: <strong>FrederikkeOgKaja2</strong></li>
                 </ul>
                 <p>Det er <strong>vigtigt</strong> at du overholder den <a href="{HtmlEncode(fixture.FromUrl.ToString())}husorden/{rulesPath}">husorden</a> der gælder for brug lokalet.</p>
                 <p><em>(Dette er en automatisk udsendt besked som ikke skal besvares.)</em></p>
                 <p>Med venlig hilsen<br>{HtmlEncode(fixture.FromName)}<br><a href="{HtmlEncode(fixture.FromUrl.ToString())}">{HtmlEncode(fixture.FromUrl.ToString())}</a></p>
                 <p><a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/bestillinger/{orderId}">Din bestilling</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/kontoudtog">Dit kontoudtog</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/konto">Din konto</a></p>
             </body>
             </html>
             """);
    }
}
