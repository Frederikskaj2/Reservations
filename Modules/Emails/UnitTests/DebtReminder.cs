using FluentAssertions;
using System.Threading.Tasks;
using Xunit;
using static Frederikskaj2.Reservations.Emails.UnitTests.HtmlEntityEncoder;

namespace Frederikskaj2.Reservations.Emails.UnitTests;

public class DebtReminder(MessageFactoryFixture fixture) : IClassFixture<MessageFactoryFixture>
{
    [Fact]
    public async Task Test()
    {
        var payment = Generate.PaymentInformation();
        var model = new DebtReminderDto(payment);

        var message = await fixture.CreateMessage(model);

        message.From.Should().Be(fixture.FromEmailAddress.ToString());
        message.ReplyTo.Should().Be(fixture.ReplyToEmailAddress.ToString());
        message.To.Should().BeEquivalentTo(fixture.ToEmailAddress.ToString());
        message.Subject.Should().Be("Manglende betaling");
        message.Body.Should().Be(
            $"""
             <!DOCTYPE html>
             <html lang="da">
             <head>
                 <title>Manglende betaling</title>
                 <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
                 <meta http-equiv="X-UA-Compatible" content="IE=edge">
                 <meta name="viewport" content="width=device-width, initial-scale=1.0">
             </head>
             <body>
                 <p>Hej {HtmlEncode(fixture.ToFullName)}</p>
                 <p>Du mangler stadig at betale en eller flere af dine bestillinger. De bliver først godkendt når du har indbetalt det beløb du skylder til grundejerforeningens bankkonto:</p>
                 <table>
                     <tbody>
                         <tr>
                             <td>Beløb:</td>
                             <td><strong>{payment.Amount.ToDecimal().ToString("N0", fixture.CultureInfo)}&#xA0;kr.</strong></td>
                         </tr>
                         <tr>
                             <td>Kontonummer:</td>
                             <td><strong>{payment.AccountNumber}</strong></td>
                         </tr>
                         <tr>
                             <td>Tekst på indbetaling:</td>
                             <td><strong>{payment.PaymentId}</strong></td>
                         </tr>
                     </tbody>
                 </table>
                 <p>Du bedes straks indbetale det skyldige beløb. Ellers forbeholder vi os ret til at annullere dine bestillinger.</p>
                 <p><strong>Bemærk:</strong> Der kan gå lidt tid fra du betaler til vi registrerer din indbetaling. Hvis du allerede har betalt, kan du se bort fra denne påmindelse.</p>
                 <p><em>(Dette er en automatisk udsendt besked som ikke skal besvares.)</em></p>
                 <p>Med venlig hilsen<br>{HtmlEncode(fixture.FromName)}<br><a href="{HtmlEncode(fixture.FromUrl.ToString())}">{HtmlEncode(fixture.FromUrl.ToString())}</a></p>
                 <p><a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/bestillinger">Dine bestillinger</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/kontoudtog">Dit kontoudtog</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/konto">Din konto</a></p>
             </body>
             </html>
             """);
    }
}
