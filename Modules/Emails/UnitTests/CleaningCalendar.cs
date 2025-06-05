using FluentAssertions;
using Frederikskaj2.Reservations.Cleaning;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using NodaTime;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static Frederikskaj2.Reservations.Cleaning.UpdateCleaningSchedule;
using static Frederikskaj2.Reservations.Emails.CleaningScheduleFunctions;
using static Frederikskaj2.Reservations.Emails.UnitTests.HtmlEntityEncoder;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Emails.UnitTests;

public class CleaningCalendar(MessageFactoryFixture fixture) : IClassFixture<MessageFactoryFixture>
{
    [Fact]
    public async Task WhenEmpty()
    {
        var overview = new CleaningScheduleOverviewDto(new([], []), new([], [], []), Resources.All);
        var model = await CreateCleaningCalendar(overview, CancellationToken.None);

        var message = await fixture.CreateMessage(model);

        message.From.Should().Be(fixture.FromEmailAddress.ToString());
        message.ReplyTo.Should().Be(fixture.ReplyToEmailAddress.ToString());
        message.To.Should().BeEquivalentTo(fixture.ToEmailAddress.ToString());
        message.Subject.Should().Be("Rengøringsplan");
        message.Body.Should().Be(
            $"""
             <!DOCTYPE html>
             <html lang="da">
             <head>
                 <title>Rengøringsplan</title>
                 <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
                 <meta http-equiv="X-UA-Compatible" content="IE=edge">
                 <meta name="viewport" content="width=device-width, initial-scale=1.0">
             </head>
             <body>
                 <p>Hej {HtmlEncode(fixture.ToFullName)}</p>
                 <p><strong>Aktuel rengøringsplan</strong></p>
                 <p>Der er ingen opgaver.</p>
                 <p><em>(Dette er en automatisk udsendt besked som ikke skal besvares.)</em></p>
                 <p>Med venlig hilsen<br>{HtmlEncode(fixture.FromName)}<br><a href="{HtmlEncode(fixture.FromUrl.ToString())}">{HtmlEncode(fixture.FromUrl.ToString())}</a></p>
             </body>
             </html>
             """);
    }

    [Fact]
    public async Task WhenHasOneTask()
    {
        var now = SystemClock.Instance.GetCurrentInstant();
        var today = now.InZone(DateTimeZoneProviders.Tzdb["Europe/Copenhagen"]).Date;
        var command = new UpdateCleaningScheduleCommand(today);
        var resourceId = Resources.All.Head().ResourceId;
        var reservationDate = today.PlusDays(1);
        var orders = Seq1(
            new Order(
                OrderId.FromInt32(1),
                UserId.FromInt32(1),
                OrderFlags.IsCleaningRequired,
                now,
                new Resident(None, Empty),
                Seq1(
                    new Reservation(
                        resourceId,
                        ReservationStatus.Confirmed,
                        new(reservationDate, 1),
                        None,
                        ReservationEmails.None)),
                Empty));
        var input = new UpdateCleaningScheduleInput(command, orders);
        var orderingOptions = new OrderingOptions
        {
            CheckInTime = LocalTime.FromHoursSinceMidnight(12),
            CheckOutTime = LocalTime.FromHoursSinceMidnight(10),
            CleaningSchedulePeriodInDays = 45,
            AdditionalDaysWhereCleaningCanBeDone = 3,
        };
        var output = UpdateCleaningScheduleCore(orderingOptions, input);
        var overview = Create(output.CleaningSchedule, new([], [], []));
        var cleaningCalendar = await CreateCleaningCalendar(overview, CancellationToken.None);

        var message = await fixture.CreateMessage(cleaningCalendar);

        message.From.Should().Be(fixture.FromEmailAddress.ToString());
        message.ReplyTo.Should().Be(fixture.ReplyToEmailAddress.ToString());
        message.To.Should().BeEquivalentTo(fixture.ToEmailAddress.ToString());
        message.Subject.Should().Be("Rengøringsplan");
        message.Body.Should().StartWith(
                $"""
                 <!DOCTYPE html>
                 <html lang="da">
                 <head>
                     <title>Rengøringsplan</title>
                     <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
                     <meta http-equiv="X-UA-Compatible" content="IE=edge">
                     <meta name="viewport" content="width=device-width, initial-scale=1.0">
                 </head>
                 <body>
                     <p>Hej {HtmlEncode(fixture.ToFullName)}</p>
                     <p><strong>Aktuel rengøringsplan</strong></p>
                     <table>
                         <thead>
                         <tr>
                             <th style="padding: 8px; text-align: left; border-top: solid 1px #CCC; border-collapse: collapse; border-bottom: solid 2px #CCC">Start</th>
                             <th style="padding: 8px; text-align: left; border-top: solid 1px #CCC; border-collapse: collapse; border-bottom: solid 2px #CCC">Slut</th>
                             <th style="padding: 8px; text-align: left; border-top: solid 1px #CCC; border-collapse: collapse; border-bottom: solid 2px #CCC">Tidsrum</th>
                             <th style="padding: 8px; text-align: left; border-top: solid 1px #CCC; border-collapse: collapse; border-bottom: solid 2px #CCC">Lokale</th>
                         </tr>
                         </thead>
                         <tbody>
                             <tr>
                                 <td style="padding: 8px; text-align: left; border-top: solid 1px #CCC; border-collapse: collapse">{HtmlEncode(FormatDate(reservationDate.PlusDays(1)))} kl. 10.00</td>
                                 <td style="padding: 8px; text-align: left; border-top: solid 1px #CCC; border-collapse: collapse">{HtmlEncode(FormatDate(reservationDate.PlusDays(4)))} kl. 12.00</td>
                                 <td style="padding: 8px; text-align: left; border-top: solid 1px #CCC; border-collapse: collapse">3 dage og 2 timer</td>
                                 <td style="padding: 8px; text-align: left; border-top: solid 1px #CCC; border-collapse: collapse">Festlokale</td>
                             </tr>
                         </tbody>
                     </table>
                     <p><strong>{HtmlEncode(FormatMonth(reservationDate.PlusDays(1)))}</strong></p>
                 """);
        message.Body.Should().EndWith(
            $"""
                 <p><em>(Dette er en automatisk udsendt besked som ikke skal besvares.)</em></p>
                 <p>Med venlig hilsen<br>{HtmlEncode(fixture.FromName)}<br><a href="{HtmlEncode(fixture.FromUrl.ToString())}">{HtmlEncode(fixture.FromUrl.ToString())}</a></p>
             </body>
             </html>
             """);
    }

    string FormatDate(LocalDate date)
    {
        var formatted = date.ToDateOnly().ToString("dddd 'den' d. MMMM yyyy", fixture.CultureInfo);
        return fixture.CultureInfo.TextInfo.ToUpper(formatted[0]) + formatted[1..];
    }

    string FormatMonth(LocalDate date) => date.ToDateOnly().ToString("yyyy-MM", fixture.CultureInfo);

    static CleaningScheduleOverviewDto Create(CleaningSchedule cleaningSchedule, CleaningTasksDelta delta) =>
        new(
            new(cleaningSchedule.CleaningTasks, cleaningSchedule.ReservedDays),
            new(delta.NewTasks, delta.CancelledTasks, delta.UpdatedTasks),
            Resources.All);
}
