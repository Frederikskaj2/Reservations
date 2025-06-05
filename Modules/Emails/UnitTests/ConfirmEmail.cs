using FluentAssertions;
using NodaTime;
using System.Threading.Tasks;
using Xunit;
using static Frederikskaj2.Reservations.Emails.UnitTests.HtmlEntityEncoder;

namespace Frederikskaj2.Reservations.Emails.UnitTests;

public class ConfirmEmail(MessageFactoryFixture fixture) : IClassFixture<MessageFactoryFixture>
{
    [Fact]
    public async Task OneDayDurationTest()
    {
        var url = Generate.Url();
        var model = new ConfirmEmailDto(url, Duration.FromDays(1));

        var message = await fixture.CreateMessage(model);

        message.From.Should().Be(fixture.FromEmailAddress.ToString());
        message.ReplyTo.Should().Be(fixture.ReplyToEmailAddress.ToString());
        message.To.Should().BeEquivalentTo(fixture.ToEmailAddress.ToString());
        message.Subject.Should().Be("Bekræft din mail");
        message.Body.Should().Be(
            $"""
             <!DOCTYPE html>
             <html lang="da">
             <head>
                 <title>Bekræft din mail</title>
                 <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
                 <meta http-equiv="X-UA-Compatible" content="IE=edge">
                 <meta name="viewport" content="width=device-width, initial-scale=1.0">
             </head>
             <body>
                 <p>Hej {HtmlEncode(fixture.ToFullName)}</p>
                 <p><a href="{HtmlEncode(url.ToString())}">Åbn linket for at bekræfte din mail.</a></p>
                 <p>Dette link er gyldigt i 1 dag.</p>
                 <p><em>(Dette er en automatisk udsendt besked som ikke skal besvares.)</em></p>
                 <p>Med venlig hilsen<br>{HtmlEncode(fixture.FromName)}<br><a href="{HtmlEncode(fixture.FromUrl.ToString())}">{HtmlEncode(fixture.FromUrl.ToString())}</a></p>
             </body>
             </html>
             """);
    }

    [Fact]
    public async Task TwoDaysDurationTest()
    {
        var url = Generate.Url();
        var model = new ConfirmEmailDto(url, Duration.FromDays(2));

        var message = await fixture.CreateMessage(model);

        message.From.Should().Be(fixture.FromEmailAddress.ToString());
        message.ReplyTo.Should().Be(fixture.ReplyToEmailAddress.ToString());
        message.To.Should().BeEquivalentTo(fixture.ToEmailAddress.ToString());
        message.Subject.Should().Be("Bekræft din mail");
        message.Body.Should().Be(
            $"""
             <!DOCTYPE html>
             <html lang="da">
             <head>
                 <title>Bekræft din mail</title>
                 <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
                 <meta http-equiv="X-UA-Compatible" content="IE=edge">
                 <meta name="viewport" content="width=device-width, initial-scale=1.0">
             </head>
             <body>
                 <p>Hej {HtmlEncode(fixture.ToFullName)}</p>
                 <p><a href="{HtmlEncode(url.ToString())}">Åbn linket for at bekræfte din mail.</a></p>
                 <p>Dette link er gyldigt i 2 dage.</p>
                 <p><em>(Dette er en automatisk udsendt besked som ikke skal besvares.)</em></p>
                 <p>Med venlig hilsen<br>{HtmlEncode(fixture.FromName)}<br><a href="{HtmlEncode(fixture.FromUrl.ToString())}">{HtmlEncode(fixture.FromUrl.ToString())}</a></p>
             </body>
             </html>
             """);
    }
}
