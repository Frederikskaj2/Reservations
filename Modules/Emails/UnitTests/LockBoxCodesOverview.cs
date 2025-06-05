using FluentAssertions;
using Frederikskaj2.Reservations.LockBox;
using NodaTime;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static Frederikskaj2.Reservations.Emails.UnitTests.HtmlEntityEncoder;
using static Frederikskaj2.Reservations.LockBox.LockBoxCodesFunctions;
using static Frederikskaj2.Reservations.LockBox.LockBoxCodesGenerator;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Emails.UnitTests;

public class LockBoxCodesOverview(MessageFactoryFixture fixture) : IClassFixture<MessageFactoryFixture>
{
    [Fact]
    public async Task Test()
    {
        var today = SystemClock.Instance.GetCurrentInstant().InZone(DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!).Date;
        var previousMonday = today.PlusDays(-((int) today.DayOfWeek - 1));
        var lockBoxCodes = CreateWeeklyLockBoxCodes(GetLockBoxCodes(new(Empty), previousMonday));
        var resources = Resources.All.OrderBy(resource => resource.Sequence).ToArray();
        var model = new LockBoxCodesOverviewDto(resources, lockBoxCodes.Map(
            weeklyLockBoxCodes => new WeeklyLockBoxCodesDto(
                weeklyLockBoxCodes.WeekNumber,
                weeklyLockBoxCodes.Date,
                weeklyLockBoxCodes.Codes.Map(
                    weeklyLockBoxCode => new WeeklyLockBoxCodeDto(
                        weeklyLockBoxCode.ResourceId,
                        weeklyLockBoxCode.Code,
                        weeklyLockBoxCode.Difference)))));

        var message = await fixture.CreateMessage(model);

        message.From.Should().Be(fixture.FromEmailAddress.ToString());
        message.ReplyTo.Should().Be(fixture.ReplyToEmailAddress.ToString());
        message.To.Should().BeEquivalentTo(fixture.ToEmailAddress.ToString());
        message.Subject.Should().Be("Nøglebokskoder");
        const string baseStyle = "padding: 8px; border-top: solid 1px #CCC; border-collapse: collapse";
        const string leftStyle = $"{baseStyle}; text-align: left";
        const string rightStyle = $"{baseStyle}; text-align: right";
        const string leftBottomBorderStyle = $"{leftStyle}; border-bottom: solid 2px #CCC";
        const string centerBottomBorderStyle = $"{baseStyle}; text-align: center; border-bottom: solid 2px #CCC";
        const string delimiter1 =
            $"""
             </th>
                             <th colspan="2" style="{centerBottomBorderStyle}">
             """;
        const string delimiter2 =
            """

                        </tr>
                        <tr>

            """;
        message.Body.Should().Be(
            $"""
             <!DOCTYPE html>
             <html lang="da">
             <head>
                 <title>Nøglebokskoder</title>
                 <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
                 <meta http-equiv="X-UA-Compatible" content="IE=edge">
                 <meta name="viewport" content="width=device-width, initial-scale=1.0">
             </head>
             <body>
                 <p>Hej {HtmlEncode(fixture.ToFullName)}</p>
                 <p>Her er de aktuelle nøglebokskoder for den kommende tid:</p>
                 <table>
                     <thead>
                         <tr>
                             <th style="{leftBottomBorderStyle}">Uge</th>
                             <th style="{leftBottomBorderStyle}">Dato</th>
                             <th colspan="2" style="{centerBottomBorderStyle}">{string.Join(delimiter1, resources.Select(resource => HtmlEncode(resource.Name)))}</th>
                         </tr>
                     </thead>
                     <tbody>
                         <tr>
             {string.Join(delimiter2, model.Codes.Select(GetRow))}
                         </tr>
                     </tbody>
                 </table>
                 <p><em>(Dette er en automatisk udsendt besked som ikke skal besvares.)</em></p>
                 <p>Med venlig hilsen<br>{HtmlEncode(fixture.FromName)}<br><a href="{HtmlEncode(fixture.FromUrl.ToString())}">{HtmlEncode(fixture.FromUrl.ToString())}</a></p>
                 <p><a href="{HtmlEncode(fixture.FromUrl.ToString())}noeglebokskoder">Nøglebokskoder</a> • <a href="{HtmlEncode(fixture.FromUrl.ToString())}bruger/konto">Din konto</a></p>
             </body>
             </html>
             """);

        string GetRow(WeeklyLockBoxCodesDto codes) =>
            $"""
                             <th style="{leftStyle}">{codes.WeekNumber}</th>
                             <td style="{leftStyle}">{codes.Date.ToDateOnly().ToString("d. MMMM yyyy", fixture.CultureInfo)}</td>
             {string.Join(Environment.NewLine, resources.Select(resource => GetCode(codes.Codes.Single(code => code.ResourceId == resource.ResourceId))))}
             """;

        string GetCode(WeeklyLockBoxCodeDto code) =>
            $"""
                             <td style="{rightStyle}">{code.Code}</td>
                             <td style="{leftStyle}">{HtmlEncode(code.Difference)}</td>
             """;
    }
}
