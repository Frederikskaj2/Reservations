using FluentAssertions;
using NodaTime;
using System.Threading.Tasks;
using Xunit;
using static Frederikskaj2.Reservations.Emails.UnitTests.HtmlEntityEncoder;

namespace Frederikskaj2.Reservations.Emails.UnitTests;

public class NewPassword(MessageFactoryFixture fixture) : IClassFixture<MessageFactoryFixture>
{
    [Fact]
    public async Task ThirtySixHoursDurationTest()
    {
        var url = Generate.Url();
        var model = new NewPasswordDto(url, Duration.FromHours(36));

        var message = await fixture.CreateMessage(model);

        message.From.Should().Be(fixture.FromEmailAddress.ToString());
        message.ReplyTo.Should().Be(fixture.ReplyToEmailAddress.ToString());
        message.To.Should().BeEquivalentTo(fixture.ToEmailAddress.ToString());
        message.Subject.Should().Be("Din anmodning om en ny adgangskode");
        message.Body.Should().Be(
            $"""
             <!DOCTYPE html>
             <html lang="da">
             <head>
                 <title>Din anmodning om en ny adgangskode</title>
                 <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
                 <meta http-equiv="X-UA-Compatible" content="IE=edge">
                 <meta name="viewport" content="width=device-width, initial-scale=1.0">
             </head>
             <body>
                 <p>Hej {HtmlEncode(fixture.ToFullName)}</p>
                 <p><a href="{HtmlEncode(url.ToString())}">Åbn linket for at oprette en ny adgangskode.</a></p>
                 <p>Dette link er gyldigt i 1 dag og 12 timer.</p>
                 <p><strong>Hvis du ikke selv har anmodet om en ny adgangskode skal du ikke foretage dig noget.</strong></p>
                 <p><em>(Dette er en automatisk udsendt besked som ikke skal besvares.)</em></p>
                 <p>Med venlig hilsen<br>{HtmlEncode(fixture.FromName)}<br><a href="{HtmlEncode(fixture.FromUrl.ToString())}">{HtmlEncode(fixture.FromUrl.ToString())}</a></p>
             </body>
             </html>
             """);
    }

    [Fact]
    public async Task SevenDaysDurationTest()
    {
        var url = Generate.Url();
        var model = new NewPasswordDto(url, Duration.FromDays(7));

        var message = await fixture.CreateMessage(model);

        message.From.Should().Be(fixture.FromEmailAddress.ToString());
        message.ReplyTo.Should().Be(fixture.ReplyToEmailAddress.ToString());
        message.To.Should().BeEquivalentTo(fixture.ToEmailAddress.ToString());
        message.Subject.Should().Be("Din anmodning om en ny adgangskode");
        message.Body.Should().Be(
            $"""
             <!DOCTYPE html>
             <html lang="da">
             <head>
                 <title>Din anmodning om en ny adgangskode</title>
                 <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
                 <meta http-equiv="X-UA-Compatible" content="IE=edge">
                 <meta name="viewport" content="width=device-width, initial-scale=1.0">
             </head>
             <body>
                 <p>Hej {HtmlEncode(fixture.ToFullName)}</p>
                 <p><a href="{HtmlEncode(url.ToString())}">Åbn linket for at oprette en ny adgangskode.</a></p>
                 <p>Dette link er gyldigt i 7 dage.</p>
                 <p><strong>Hvis du ikke selv har anmodet om en ny adgangskode skal du ikke foretage dig noget.</strong></p>
                 <p><em>(Dette er en automatisk udsendt besked som ikke skal besvares.)</em></p>
                 <p>Med venlig hilsen<br>{HtmlEncode(fixture.FromName)}<br><a href="{HtmlEncode(fixture.FromUrl.ToString())}">{HtmlEncode(fixture.FromUrl.ToString())}</a></p>
             </body>
             </html>
             """);
    }
}
