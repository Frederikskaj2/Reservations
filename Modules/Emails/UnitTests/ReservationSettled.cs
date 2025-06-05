using FluentAssertions;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Users;
using System.Threading.Tasks;
using Xunit;
using static Frederikskaj2.Reservations.Emails.UnitTests.HtmlEntityEncoder;

namespace Frederikskaj2.Reservations.Emails.UnitTests;

public class ReservationSettled(MessageFactoryFixture fixture) : IClassFixture<MessageFactoryFixture>
{
    [Fact]
    public async Task WithFullDepositRefunded()
    {
        var orderId = Generate.OrderId();
        var reservation = Generate.ReservationDescription();
        var amount = Generate.Amount();
        var model = new ReservationSettledDto(orderId, reservation, amount, Amount.Zero, Description: null);

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
                 <p>Vi har nu opgjort din reservation af {HtmlEncode(Resources.GetNameUnsafe(reservation.ResourceId))} {HtmlEncode(reservation.Date.ToDateOnly().ToString("dddd 'den' d. MMMM yyyy", fixture.CultureInfo))} hørende til <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/bestillinger/{orderId}">bestillingsnr.&#xA0;{orderId}</a>.</p>
                 <p>Du vil få udbetalt dit depositum som udgør {amount.ToDecimal().ToString("N0", fixture.CultureInfo)}&#xA0;kr.</p>
                 <p><strong>Bemærk:</strong> Vi udbetaler én gang om måneden, og der kan derfor godt gå noget tid før vi udbetaler dit tilgodehavende. Du vil få besked når udbetalingen gennemføres.</p>
                 <p><em>(Dette er en automatisk udsendt besked som ikke skal besvares.)</em></p>
                 <p>Med venlig hilsen<br>{HtmlEncode(fixture.FromName)}<br><a href="{HtmlEncode(fixture.FromUrl.ToString())}">{HtmlEncode(fixture.FromUrl.ToString())}</a></p>
                 <p><a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/bestillinger/{orderId}">Din bestilling</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/kontoudtog">Dit kontoudtog</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/konto">Din konto</a></p>
             </body>
             </html>
             """);
    }

    [Fact]
    public async Task WithPartialDepositRefunded()
    {
        var orderId = Generate.OrderId();
        var reservation = Generate.ReservationDescription();
        var amount = Generate.Amount();
        var damages = Generate.AmountBelow(amount);
        var description = Generate.DamagesDescription();
        var model = new ReservationSettledDto(orderId, reservation, amount, damages, description);

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
                 <p>Vi har nu opgjort din reservation af {HtmlEncode(Resources.GetNameUnsafe(reservation.ResourceId))} {HtmlEncode(reservation.Date.ToDateOnly().ToString("dddd 'den' d. MMMM yyyy", fixture.CultureInfo))} hørende til <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/bestillinger/{orderId}">bestillingsnr.&#xA0;{orderId}</a>.</p>
                 <p>Vi tilbageholder {damages.ToDecimal().ToString("N0", fixture.CultureInfo)}&#xA0;kr. af dit depositum på {amount.ToDecimal().ToString("N0", fixture.CultureInfo)}&#xA0;kr. til dækning af følgende: {description}.</p>
                 <p>Du vil få udbetalt resten af dit depositum som udgør {(amount - damages).ToDecimal().ToString("N0", fixture.CultureInfo)}&#xA0;kr.</p>
                 <p><strong>Bemærk:</strong> Vi udbetaler én gang om måneden, og der kan derfor godt gå noget tid før vi udbetaler dit tilgodehavende. Du vil få besked når udbetalingen gennemføres.</p>
                 <p><em>(Dette er en automatisk udsendt besked som ikke skal besvares.)</em></p>
                 <p>Med venlig hilsen<br>{HtmlEncode(fixture.FromName)}<br><a href="{HtmlEncode(fixture.FromUrl.ToString())}">{HtmlEncode(fixture.FromUrl.ToString())}</a></p>
                 <p><a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/bestillinger/{orderId}">Din bestilling</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/kontoudtog">Dit kontoudtog</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/konto">Din konto</a></p>
             </body>
             </html>
             """);
    }

    [Fact]
    public async Task WithNoDepositRefunded()
    {
        var orderId = Generate.OrderId();
        var reservation = Generate.ReservationDescription();
        var amount = Generate.Amount();
        var description = Generate.DamagesDescription();
        var model = new ReservationSettledDto(orderId, reservation, amount, amount, description);

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
                 <p>Vi har nu opgjort din reservation af {HtmlEncode(Resources.GetNameUnsafe(reservation.ResourceId))} {HtmlEncode(reservation.Date.ToDateOnly().ToString("dddd 'den' d. MMMM yyyy", fixture.CultureInfo))} hørende til <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/bestillinger/{orderId}">bestillingsnr.&#xA0;{orderId}</a>.</p>
                 <p>Vi tilbageholder {amount.ToDecimal().ToString("N0", fixture.CultureInfo)}&#xA0;kr. af dit depositum på {amount.ToDecimal().ToString("N0", fixture.CultureInfo)}&#xA0;kr. til dækning af følgende: {description}.</p>
                 <p><em>(Dette er en automatisk udsendt besked som ikke skal besvares.)</em></p>
                 <p>Med venlig hilsen<br>{HtmlEncode(fixture.FromName)}<br><a href="{HtmlEncode(fixture.FromUrl.ToString())}">{HtmlEncode(fixture.FromUrl.ToString())}</a></p>
                 <p><a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/bestillinger/{orderId}">Din bestilling</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/kontoudtog">Dit kontoudtog</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/konto">Din konto</a></p>
             </body>
             </html>
             """);
    }}
