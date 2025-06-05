using FluentAssertions;
using Frederikskaj2.Reservations.Bank;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using System.Threading.Tasks;
using Xunit;
using static Frederikskaj2.Reservations.Emails.UnitTests.HtmlEntityEncoder;

namespace Frederikskaj2.Reservations.Emails.UnitTests;

public class PostingsForMonth(MessageFactoryFixture fixture) : IClassFixture<MessageFactoryFixture>
{
    [Fact]
    public async Task Test()
    {
        var month = Generate.Month();
        var accountNames = new AccountName[]
        {
            new(PostingAccount.Income, "1201 \u2013 Udlejning af fælleslokale"),
            new(PostingAccount.Bank, "4021 \u2013 Bank"),
            new(PostingAccount.AccountsReceivable, "4401 \u2013 Tilgodehavende fælleslokaler"),
            new(PostingAccount.Deposits, "7601 \u2013 Depositum (IND)"),
            new(PostingAccount.AccountsPayable, "7601 \u2013 Depositum (UD)"),
        };
        var fullName = Generate.FullName();
        var paymentId = Generate.PaymentId();
        var orderId = Generate.OrderId();
        var postings = new PostingDto[]
        {
            new(
                0,
                month,
                Activity.PlaceOrder,
                0,
                fullName,
                paymentId,
                orderId,
                [
                    new(PostingAccount.Income, Amount.FromInt32(-850)),
                    new(PostingAccount.AccountsReceivable, Amount.FromInt32(1350)),
                    new(PostingAccount.Deposits, Amount.FromInt32(-500)),
                ]),
        };
        var model = new PostingsForMonthDto(month, accountNames, postings);

        var message = await fixture.CreateMessage(model);

        message.From.Should().Be(fixture.FromEmailAddress.ToString());
        message.ReplyTo.Should().Be(fixture.ReplyToEmailAddress.ToString());
        message.To.Should().BeEquivalentTo(fixture.ToEmailAddress.ToString());
        message.Subject.Should().Be($"Posteringer for {month.ToDateOnly().ToString("yyyy-MM", fixture.CultureInfo)}");
        message.Body.Should().Be(
            $"""
             <!DOCTYPE html>
             <html lang="da">
             <head>
                 <title>Posteringer for {month.ToDateOnly().ToString("yyyy-MM", fixture.CultureInfo)}</title>
                 <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
                 <meta http-equiv="X-UA-Compatible" content="IE=edge">
                 <meta name="viewport" content="width=device-width, initial-scale=1.0">
             </head>
             <body>
                 <p>Hej {HtmlEncode(fixture.ToFullName)}</p>
                 <p>Her er posteringerne for {month.ToDateOnly().ToString("yyyy-MM", fixture.CultureInfo)}.</p>
                 <table>
                     <thead>
                         <tr>
                             <th style="padding: 8px; text-align: left; border-top: solid 1px #CCC; border-bottom: solid 2px #CCC; border-collapse: collapse">Dato</th>
                             <th style="padding: 8px; text-align: left; border-top: solid 1px #CCC; border-bottom: solid 2px #CCC; border-collapse: collapse">Beskrivelse</th>
                             <th style="padding: 8px; text-align: left; border-top: solid 1px #CCC; border-bottom: solid 2px #CCC; border-collapse: collapse">Konto</th>
                             <th style="padding: 8px; text-align: right; border-top: solid 1px #CCC; border-bottom: solid 2px #CCC; border-collapse: collapse">Debet</th>
                             <th style="padding: 8px; text-align: right; border-top: solid 1px #CCC; border-bottom: solid 2px #CCC; border-collapse: collapse">Kredit</th>
                         </tr>
                     </thead>
                     <tbody>
                         <tr>
                             <td style="padding: 8px; text-align: left; border-top: solid 1px #CCC; border-collapse: collapse">{month.ToDateOnly().ToString("yyyy-MM-dd", fixture.CultureInfo)}</td>
                             <td style="padding: 8px; text-align: left; border-top: solid 1px #CCC; border-collapse: collapse">Oprettelse af bestilling {orderId} ({HtmlEncode(fullName)} {HtmlEncode(paymentId.ToString())})</td>
                             <td style="padding: 8px; text-align: left; border-top: solid 1px #CCC; border-collapse: collapse">1201 &#x2013; Udlejning af f&#xE6;lleslokale</td>
                             <td style="padding: 8px; text-align: right; border-top: solid 1px #CCC; border-collapse: collapse"></td>
                             <td style="padding: 8px; text-align: right; border-top: solid 1px #CCC; border-collapse: collapse">850</td>
                         </tr>
                         <tr>
                             <td colspan="2" style="padding: 8px; border-collapse: collapse"></td>
                             <td style="padding: 8px; text-align: left; border-top: solid 1px #CCC; border-collapse: collapse">4401 &#x2013; Tilgodehavende f&#xE6;lleslokaler</td>
                             <td style="padding: 8px; text-align: right; border-top: solid 1px #CCC; border-collapse: collapse">1.350</td>
                             <td style="padding: 8px; text-align: right; border-top: solid 1px #CCC; border-collapse: collapse"></td>
                         </tr>
                         <tr>
                             <td colspan="2" style="padding: 8px; border-collapse: collapse; border-bottom: solid 2px #CCC"></td>
                             <td style="padding: 8px; text-align: left; border-top: solid 1px #CCC; border-collapse: collapse; border-bottom: solid 2px #CCC">7601 &#x2013; Depositum (IND)</td>
                             <td style="padding: 8px; text-align: right; border-top: solid 1px #CCC; border-collapse: collapse; border-bottom: solid 2px #CCC"></td>
                             <td style="padding: 8px; text-align: right; border-top: solid 1px #CCC; border-collapse: collapse; border-bottom: solid 2px #CCC">500</td>
                         </tr>
                     </tbody>
                 </table>
                 <p><em>(Dette er en automatisk udsendt besked som ikke skal besvares.)</em></p>
                 <p>Med venlig hilsen<br>{HtmlEncode(fixture.FromName)}<br><a href="{HtmlEncode(fixture.FromUrl.ToString())}">{HtmlEncode(fixture.FromUrl.ToString())}</a></p>
                 <p><a href="{HtmlEncode(fixture.FromUrl.ToString())}posteringer">Posteringer</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/konto">Din konto</a></p>
             </body>
             </html>
             """);
    }
}
