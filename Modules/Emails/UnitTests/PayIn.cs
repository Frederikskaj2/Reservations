using FluentAssertions;
using System.Threading.Tasks;
using Xunit;
using static Frederikskaj2.Reservations.Emails.UnitTests.HtmlEntityEncoder;

namespace Frederikskaj2.Reservations.Emails.UnitTests;

public class PayIn(MessageFactoryFixture fixture) : IClassFixture<MessageFactoryFixture>
{
    [Fact]
    public async Task WithPaymentTest()
    {
        var amount = Generate.Amount();
        var payment = Generate.PaymentInformation();
        var model = new PayInDto(amount, payment);

        var message = await fixture.CreateMessage(model);

        message.From.Should().Be(fixture.FromEmailAddress.ToString());
        message.ReplyTo.Should().Be(fixture.ReplyToEmailAddress.ToString());
        message.To.Should().BeEquivalentTo(fixture.ToEmailAddress.ToString());
        message.Subject.Should().Be("Indbetaling");
        message.Body.Should().Be(
            $"""
             <!DOCTYPE html>
             <html lang="da">
             <head>
                 <title>Indbetaling</title>
                 <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
                 <meta http-equiv="X-UA-Compatible" content="IE=edge">
                 <meta name="viewport" content="width=device-width, initial-scale=1.0">
             </head>
             <body>
                 <p>Hej {HtmlEncode(fixture.ToFullName)}</p>
                 <p>Vi har modtaget din indbetaling på {amount.ToDecimal().ToString("N0", fixture.CultureInfo)}&#xA0;kr.</p>
                 <p>Da du stadig skylder et beløb for dine bestillinger beder vi dig indbetale den resterende gæld til grundejerforeningens bankkonto:</p>
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
                 <p><em>(Dette er en automatisk udsendt besked som ikke skal besvares.)</em></p>
                 <p>Med venlig hilsen<br>{HtmlEncode(fixture.FromName)}<br><a href="{HtmlEncode(fixture.FromUrl.ToString())}">{HtmlEncode(fixture.FromUrl.ToString())}</a></p>
                 <p><a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/bestillinger">Dine bestillinger</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/kontoudtog">Dit kontoudtog</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/konto">Din konto</a></p>
             </body>
             </html>
             """);
    }

    [Fact]
    public async Task WithoutPaymentTest()
    {
        var amount = Generate.Amount();
        var model = new PayInDto(amount, Payment: null);

        var message = await fixture.CreateMessage(model);

        message.From.Should().Be(fixture.FromEmailAddress.ToString());
        message.ReplyTo.Should().Be(fixture.ReplyToEmailAddress.ToString());
        message.To.Should().BeEquivalentTo(fixture.ToEmailAddress.ToString());
        message.Subject.Should().Be("Indbetaling");
        message.Body.Should().Be(
            $"""
             <!DOCTYPE html>
             <html lang="da">
             <head>
                 <title>Indbetaling</title>
                 <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
                 <meta http-equiv="X-UA-Compatible" content="IE=edge">
                 <meta name="viewport" content="width=device-width, initial-scale=1.0">
             </head>
             <body>
                 <p>Hej {HtmlEncode(fixture.ToFullName)}</p>
                 <p>Vi har modtaget din indbetaling på {amount.ToDecimal().ToString("N0", fixture.CultureInfo)}&#xA0;kr.</p>
                 <p><em>(Dette er en automatisk udsendt besked som ikke skal besvares.)</em></p>
                 <p>Med venlig hilsen<br>{HtmlEncode(fixture.FromName)}<br><a href="{HtmlEncode(fixture.FromUrl.ToString())}">{HtmlEncode(fixture.FromUrl.ToString())}</a></p>
                 <p><a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/bestillinger">Dine bestillinger</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/kontoudtog">Dit kontoudtog</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/konto">Din konto</a></p>
             </body>
             </html>
             """);
    }
}
