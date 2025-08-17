using Frederikskaj2.Reservations.Users;
using NodaTime;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Bank;

static class SendDebtReminders
{
    public static SendDebtReminderOutput SendDebtReminderCore(SendDebtRemindersInput input) =>
        new(
            input.UsersWithReminder.Map(user => UpdateDebtReminder(input.Command.Timestamp, user)),
            input.UsersWithReminder.Filter(user => user.HasDebt()),
            new(input.Command.Timestamp));

    static User UpdateDebtReminder(Instant timestamp, User user) =>
        user.HasDebt()
            ? user with { LatestDebtReminder = timestamp }
            : user with { LatestDebtReminder = None };
}
