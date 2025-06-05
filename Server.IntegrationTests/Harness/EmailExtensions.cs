using Frederikskaj2.Reservations.Emails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

static class EmailExtensions
{
    public static async ValueTask<IEnumerable<Email>> DequeueEmails(this SessionFixture session)
    {
        var queueClient = session.QueueClient;
        for (var i = 0; i < 80; i += 1)
        {
            var response = await queueClient.ReceiveMessagesAsync(32);
            if (response.Value.Length > 0)
                return (await response.Value.ToAsyncEnumerable()
                    .SelectAwait(message => session.Deserialize<Email>(message.Body.ToStream()))
                    .ToListAsync())!;
            await Task.Delay(TimeSpan.FromMilliseconds(25));
        }

        return [];
    }

    public static async ValueTask<Email> DequeueConfirmEmailEmail(this SessionFixture session)
    {
        var emails = await session.DequeueEmails();
        return emails.Last(email => email.ConfirmEmail is not null);
    }

    public static async ValueTask<Email> DequeueNewPasswordEmail(this SessionFixture session)
    {
        var emails = await session.DequeueEmails();
        return emails.Last(email => email.NewPassword is not null);
    }

    public static async ValueTask<Email> DequeueUserDeletedEmail(this SessionFixture session)
    {
        var emails = await session.DequeueEmails();
        return emails.Last(email => email.UserDeleted is not null);
    }

    public static Email? OrderReceived(this IEnumerable<Email> emails) =>
        emails.SingleOrDefault(email => email.OrderReceived is not null);

    public static Email? NewOrder(this IEnumerable<Email> emails) =>
        emails.SingleOrDefault(email => email.NewOrder is not null);

    public static Email? PayIn(this IEnumerable<Email> emails) =>
        emails.SingleOrDefault(email => email.PayIn is not null);

    public static Email? ReservationsCancelled(this IEnumerable<Email> emails) =>
        emails.SingleOrDefault(email => email.ReservationsCancelled is not null);

    public static Email? LockBoxCodesOverview(this IEnumerable<Email> emails) =>
        emails.SingleOrDefault(email => email.LockBoxCodesOverview is not null);
}
