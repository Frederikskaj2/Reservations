using FluentAssertions;
using System.Threading.Tasks;
using Xunit;
using static Frederikskaj2.Reservations.Emails.UnitTests.HtmlEntityEncoder;

namespace Frederikskaj2.Reservations.Emails.UnitTests;

public class OrderReceived(MessageFactoryFixture fixture) : IClassFixture<MessageFactoryFixture>
{
    [Fact]
    public async Task WithPaymentTest()
    {
        var orderId = Generate.OrderId();
        var payment = Generate.PaymentInformation();
        var model = new OrderReceivedDto(orderId, payment);

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
                 <p>Tak for din bestilling med <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/bestillinger/{orderId}">bestillingsnr.&#xA0;{orderId}</a>.</p>
                 <p>Din bestilling bliver først godkendt når du har indbetalt det beløb du skylder til grundejerforeningens bankkonto:</p>
                 <table>
                     <tbody>
                         <tr>
                             <td>Beløb:</td>
                             <td><strong>{model.Payment!.Amount.ToDecimal().ToString("N0", fixture.CultureInfo)}&#xA0;kr.</strong></td>
                         </tr>
                         <tr>
                             <td>Kontonummer:</td>
                             <td><strong>{model.Payment.AccountNumber}</strong></td>
                         </tr>
                         <tr>
                             <td>Tekst på indbetaling:</td>
                             <td><strong>{model.Payment.PaymentId}</strong></td>
                         </tr>
                     </tbody>
                 </table>
                 <p>Du bedes <strong>inden fire dage</strong> indbetale det skyldige beløb. Ellers forbeholder vi os ret til at annullere din bestilling.</p>
                 <p><strong>Bemærk:</strong> Hvis du har flere ubetalte bestillinger udgør ovenstående din <strong>samlede</strong> gæld til grundejerforeningen. Du skal kun betale det beløb som fremgår af den <strong>seneste</strong> besked du har modtaget.</p>
                 <p><em>(Dette er en automatisk udsendt besked som ikke skal besvares.)</em></p>
                 <p>Med venlig hilsen<br>{HtmlEncode(fixture.FromName)}<br><a href="{HtmlEncode(fixture.FromUrl.ToString())}">{HtmlEncode(fixture.FromUrl.ToString())}</a></p>
                 <p><a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/bestillinger/{orderId}">Din bestilling</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/kontoudtog">Dit kontoudtog</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/konto">Din konto</a></p>
             </body>
             </html>
             """);
    }

    [Fact]
    public async Task WithoutPaymentTest()
    {
        var orderId = Generate.OrderId();
        var model = new OrderReceivedDto(orderId, Payment: null);

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
                 <p>Tak for din bestilling med <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/bestillinger/{orderId}">bestillingsnr.&#xA0;{orderId}</a>.</p>
                 <p>Da du tidligere har indbetalt et beløb som dækker prisen for denne bestilling, er din bestilling godkendt og betalt, og du behøver ikke at foretage dig yderligere.</p>
                 <p><em>(Dette er en automatisk udsendt besked som ikke skal besvares.)</em></p>
                 <p>Med venlig hilsen<br>{HtmlEncode(fixture.FromName)}<br><a href="{HtmlEncode(fixture.FromUrl.ToString())}">{HtmlEncode(fixture.FromUrl.ToString())}</a></p>
                 <p><a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/bestillinger/{orderId}">Din bestilling</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/kontoudtog">Dit kontoudtog</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/konto">Din konto</a></p>
             </body>
             </html>
             """);
    }
}
