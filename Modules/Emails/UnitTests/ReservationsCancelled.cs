using FluentAssertions;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Users;
using System.Threading.Tasks;
using Xunit;
using static Frederikskaj2.Reservations.Emails.UnitTests.HtmlEntityEncoder;

namespace Frederikskaj2.Reservations.Emails.UnitTests;

public class ReservationsCancelled(MessageFactoryFixture fixture) : IClassFixture<MessageFactoryFixture>
{
    [Fact]
    public async Task WithNoRefundAndNoFeeTest()
    {
        var orderId = Generate.OrderId();
        var reservations = new[] { Generate.ReservationDescription(), Generate.ReservationDescription() };
        var model = new ReservationsCancelledDto(orderId, reservations, Amount.Zero, Amount.Zero);

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
                 <p>Følgende reservationer hørende til <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/bestillinger/{orderId}">bestillingsnr.&#xA0;{orderId}</a> er blevet annulleret:</p>
                 <ul>
                     <li>{HtmlEncode(Resources.GetNameUnsafe(reservations[0].ResourceId))} {HtmlEncode(reservations[0].Date.ToDateOnly().ToString("dddd 'den' d. MMMM yyyy", fixture.CultureInfo))}</li>
                     <li>{HtmlEncode(Resources.GetNameUnsafe(reservations[1].ResourceId))} {HtmlEncode(reservations[1].Date.ToDateOnly().ToString("dddd 'den' d. MMMM yyyy", fixture.CultureInfo))}</li>
                 </ul>
                 <p><em>(Dette er en automatisk udsendt besked som ikke skal besvares.)</em></p>
                 <p>Med venlig hilsen<br>{HtmlEncode(fixture.FromName)}<br><a href="{HtmlEncode(fixture.FromUrl.ToString())}">{HtmlEncode(fixture.FromUrl.ToString())}</a></p>
                 <p><a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/bestillinger/{orderId}">Din bestilling</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/kontoudtog">Dit kontoudtog</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/konto">Din konto</a></p>
             </body>
             </html>
             """);
    }

    [Fact]
    public async Task WithRefundAndNoFeeTest()
    {
        var orderId = Generate.OrderId();
        var reservations = new[] { Generate.ReservationDescription(), Generate.ReservationDescription() };
        var refund = Generate.Amount();
        var model = new ReservationsCancelledDto(orderId, reservations, refund, Amount.Zero);

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
                 <p>Følgende reservationer hørende til <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/bestillinger/{orderId}">bestillingsnr.&#xA0;{orderId}</a> er blevet annulleret:</p>
                 <ul>
                     <li>{HtmlEncode(Resources.GetNameUnsafe(reservations[0].ResourceId))} {HtmlEncode(reservations[0].Date.ToDateOnly().ToString("dddd 'den' d. MMMM yyyy", fixture.CultureInfo))}</li>
                     <li>{HtmlEncode(Resources.GetNameUnsafe(reservations[1].ResourceId))} {HtmlEncode(reservations[1].Date.ToDateOnly().ToString("dddd 'den' d. MMMM yyyy", fixture.CultureInfo))}</li>
                 </ul>
                 <p>
                     Leje og depositum på {refund.ToDecimal().ToString("N0", fixture.CultureInfo)}&#xA0;kr. vil blive refunderet.
                 </p>
                 <p><em>(Dette er en automatisk udsendt besked som ikke skal besvares.)</em></p>
                 <p>Med venlig hilsen<br>{HtmlEncode(fixture.FromName)}<br><a href="{HtmlEncode(fixture.FromUrl.ToString())}">{HtmlEncode(fixture.FromUrl.ToString())}</a></p>
                 <p><a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/bestillinger/{orderId}">Din bestilling</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/kontoudtog">Dit kontoudtog</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/konto">Din konto</a></p>
             </body>
             </html>
             """);
    }

    [Fact]
    public async Task WithRefundAndFeeTest()
    {
        var orderId = Generate.OrderId();
        var reservations = new[] { Generate.ReservationDescription(), Generate.ReservationDescription() };
        var refund = Generate.Amount();
        var fee = Generate.Amount();
        var model = new ReservationsCancelledDto(orderId, reservations, refund, fee);

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
                 <p>Følgende reservationer hørende til <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/bestillinger/{orderId}">bestillingsnr.&#xA0;{orderId}</a> er blevet annulleret:</p>
                 <ul>
                     <li>{HtmlEncode(Resources.GetNameUnsafe(reservations[0].ResourceId))} {HtmlEncode(reservations[0].Date.ToDateOnly().ToString("dddd 'den' d. MMMM yyyy", fixture.CultureInfo))}</li>
                     <li>{HtmlEncode(Resources.GetNameUnsafe(reservations[1].ResourceId))} {HtmlEncode(reservations[1].Date.ToDateOnly().ToString("dddd 'den' d. MMMM yyyy", fixture.CultureInfo))}</li>
                 </ul>
                 <p>
                     Leje og depositum på {refund.ToDecimal().ToString("N0", fixture.CultureInfo)}&#xA0;kr. vil blive refunderet.
                     Vi har fratrukket et afbestillingsgebyr på {fee.ToDecimal().ToString("N0", fixture.CultureInfo)}&#xA0;kr.
                 </p>
                 <p><em>(Dette er en automatisk udsendt besked som ikke skal besvares.)</em></p>
                 <p>Med venlig hilsen<br>{HtmlEncode(fixture.FromName)}<br><a href="{HtmlEncode(fixture.FromUrl.ToString())}">{HtmlEncode(fixture.FromUrl.ToString())}</a></p>
                 <p><a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/bestillinger/{orderId}">Din bestilling</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/kontoudtog">Dit kontoudtog</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/konto">Din konto</a></p>
             </body>
             </html>
             """);
    }
}
