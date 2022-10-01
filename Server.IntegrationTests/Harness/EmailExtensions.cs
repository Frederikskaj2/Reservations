using Frederikskaj2.Reservations.Shared.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

static class EmailsExtensions
{
    public static async ValueTask<IEnumerable<Email>> DequeueEmailsAsync(this SessionFixture session)
    {
        var queueClient = session.QueueClient;
        for (var i = 0; i < 80; i += 1)
        {
            var response = await queueClient.ReceiveMessagesAsync(32);
            if (response.Value.Length > 0)
                return (await response.Value.ToAsyncEnumerable()
                    .SelectAwait(message => session.DeserializeAsync<Email>(message.Body.ToStream()))
                    .ToListAsync())!;
            await Task.Delay(TimeSpan.FromMilliseconds(25));
        }

        return Enumerable.Empty<Email>();
    }

    public static async ValueTask<ConfirmEmail> DequeueConfirmEmailEmailAsync(this SessionFixture session)
    {
        var emails = await session.DequeueEmailsAsync();
        return emails.Last(email => email.ConfirmEmail is not null).ConfirmEmail!;
    }

    public static async ValueTask<NewPassword> DequeueNewPasswordEmailAsync(this SessionFixture session)
    {
        var emails = await session.DequeueEmailsAsync();
        return emails.Last(email => email.NewPassword is not null).NewPassword!;
    }

    public static async ValueTask<UserDeleted> DequeueUserDeletedEmailAsync(this SessionFixture session)
    {
        var emails = await session.DequeueEmailsAsync();
        return emails.Last(email => email.UserDeleted is not null).UserDeleted!;
    }

    public static NewOrder NewOrder(this IEnumerable<Email> emails) =>
        emails.Single(email => email.NewOrder is not null).NewOrder!;

    public static OrderReceived OrderReceived(this IEnumerable<Email> emails) =>
        emails.Single(email => email.OrderReceived is not null).OrderReceived!;

    public static ReservationsCancelled ReservationsCancelled(this IEnumerable<Email> emails) =>
        emails.Single(email => email.ReservationsCancelled is not null).ReservationsCancelled!;
}
