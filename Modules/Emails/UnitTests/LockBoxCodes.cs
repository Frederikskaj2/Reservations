using FluentAssertions;
using Frederikskaj2.Reservations.LockBox;
using System.Threading.Tasks;
using Xunit;
using static Frederikskaj2.Reservations.Emails.UnitTests.HtmlEntityEncoder;

namespace Frederikskaj2.Reservations.Emails.UnitTests;

public class LockBoxCodes(MessageFactoryFixture fixture) : IClassFixture<MessageFactoryFixture>
{
    [Fact]
    public async Task WithOneCode()
    {
        var orderId = Generate.OrderId();
        var resourceId = Generate.ResourceId();
        var datedLockBoxCode = Generate.DatedLockBoxCode();
        var date = datedLockBoxCode.Date;
        var model = new LockBoxCodesDto(orderId, resourceId, date, [datedLockBoxCode]);

        var message = await fixture.CreateMessage(model);

        message.From.Should().Be(fixture.FromEmailAddress.ToString());
        message.ReplyTo.Should().Be(fixture.ReplyToEmailAddress.ToString());
        message.To.Should().BeEquivalentTo(fixture.ToEmailAddress.ToString());
        message.Subject.Should().Be($"Nøglebokskode til {Resources.GetNameUnsafe(resourceId)} den {date.ToDateOnly().ToString("d. MMMM yyyy", fixture.CultureInfo)}");
        var rulesPath = Resources.GetResourceType(resourceId) == ResourceType.BanquetFacilities ? "festlokale" : "sovevaerelser";
        message.Body.Should().Be(
            $"""
             <!DOCTYPE html>
             <html lang="da">
             <head>
                 <title>Nøglebokskode til {HtmlEncode(Resources.GetNameUnsafe(resourceId))} den {date.ToDateOnly().ToString("d. MMMM yyyy", fixture.CultureInfo)}</title>
                 <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
                 <meta http-equiv="X-UA-Compatible" content="IE=edge">
                 <meta name="viewport" content="width=device-width, initial-scale=1.0">
             </head>
             <body>
                 <p>Hej {HtmlEncode(fixture.ToFullName)}</p>
                 <p>Her er den nøglebokskode du skal bruge for at få adgang til <strong>{HtmlEncode(Resources.GetNameUnsafe(resourceId))}</strong> den {date.ToDateOnly().ToString("d. MMMM yyyy", fixture.CultureInfo)}:</p>
                 <p><strong>{datedLockBoxCode.Code}</strong></p>
                 <p>Det er <strong>vigtigt</strong> at du overholder den <a href="{HtmlEncode(fixture.FromUrl.ToString())}husorden/{rulesPath}">husorden</a> der gælder for brug lokalet.</p>
                 <p><em>(Dette er en automatisk udsendt besked som ikke skal besvares.)</em></p>
                 <p>Med venlig hilsen<br>{HtmlEncode(fixture.FromName)}<br><a href="{HtmlEncode(fixture.FromUrl.ToString())}">{HtmlEncode(fixture.FromUrl.ToString())}</a></p>
                 <p><a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/bestillinger/{orderId}">Din bestilling</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/kontoudtog">Dit kontoudtog</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/konto">Din konto</a></p>
             </body>
             </html>
             """);
    }

    [Fact]
    public async Task WithTwoCodes()
    {
        var orderId = Generate.OrderId();
        var resourceId = Generate.ResourceId();
        var datedLockBoxCode1 = Generate.DatedLockBoxCode();
        var datedLockBoxCode2 = Generate.DatedLockBoxCode();
        var date = datedLockBoxCode1.Date;
        var model = new LockBoxCodesDto(orderId, resourceId, date, [datedLockBoxCode1, datedLockBoxCode2]);

        var message = await fixture.CreateMessage(model);

        message.From.Should().Be(fixture.FromEmailAddress.ToString());
        message.ReplyTo.Should().Be(fixture.ReplyToEmailAddress.ToString());
        message.To.Should().BeEquivalentTo(fixture.ToEmailAddress.ToString());
        message.Subject.Should().Be($"Nøglebokskode til {Resources.GetNameUnsafe(resourceId)} den {date.ToDateOnly().ToString("d. MMMM yyyy", fixture.CultureInfo)}");
        var rulesPath = Resources.GetResourceType(resourceId) == ResourceType.BanquetFacilities ? "festlokale" : "sovevaerelser";
        message.Body.Should().Be(
            $"""
             <!DOCTYPE html>
             <html lang="da">
             <head>
                 <title>Nøglebokskode til {HtmlEncode(Resources.GetNameUnsafe(resourceId))} den {date.ToDateOnly().ToString("d. MMMM yyyy", fixture.CultureInfo)}</title>
                 <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
                 <meta http-equiv="X-UA-Compatible" content="IE=edge">
                 <meta name="viewport" content="width=device-width, initial-scale=1.0">
             </head>
             <body>
                 <p>Hej {HtmlEncode(fixture.ToFullName)}</p>
                 <p>Her er de nøglebokskoder du skal bruge for at få adgang til <strong>{HtmlEncode(Resources.GetNameUnsafe(resourceId))}</strong> den {date.ToDateOnly().ToString("d. MMMM yyyy", fixture.CultureInfo)}:</p>
                 <p>Fra {datedLockBoxCode1.Date.ToDateOnly().ToString("d. MMMM yyyy", fixture.CultureInfo)}: <strong>{datedLockBoxCode1.Code}</strong></p>
                 <p>Fra {datedLockBoxCode2.Date.ToDateOnly().ToString("d. MMMM yyyy", fixture.CultureInfo)}: <strong>{datedLockBoxCode2.Code}</strong></p>
                 <p>Det er <strong>vigtigt</strong> at du overholder den <a href="{HtmlEncode(fixture.FromUrl.ToString())}husorden/{rulesPath}">husorden</a> der gælder for brug lokalet.</p>
                 <p><em>(Dette er en automatisk udsendt besked som ikke skal besvares.)</em></p>
                 <p>Med venlig hilsen<br>{HtmlEncode(fixture.FromName)}<br><a href="{HtmlEncode(fixture.FromUrl.ToString())}">{HtmlEncode(fixture.FromUrl.ToString())}</a></p>
                 <p><a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/bestillinger/{orderId}">Din bestilling</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/kontoudtog">Dit kontoudtog</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/konto">Din konto</a></p>
             </body>
             </html>
             """);
    }
}
