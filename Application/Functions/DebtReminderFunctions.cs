using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;

namespace Frederikskaj2.Reservations.Application;

static class DebtReminderFunctions
{
    public static IPersistenceContext TryUpdateLatestDebtReminder(IPersistenceContext context, Instant timestamp) =>
        context.UpdateItem<User>(user => TryUpdateLatestDebtReminder(user, timestamp));

    static User TryUpdateLatestDebtReminder(User user, Instant timestamp) =>
        user.Balance() > Amount.Zero
            ? TrySetLatestDebtReminder(user, timestamp)
            : TryClearLatestDebtReminder(user);

    static User TrySetLatestDebtReminder(User user, Instant timestamp) =>
        !user.LatestDebtReminder.HasValue ? user with { LatestDebtReminder = timestamp } : user;

    static User TryClearLatestDebtReminder(User user) =>
        user.LatestDebtReminder.HasValue
            ? user with { LatestDebtReminder = null }
            : user;

    public static EitherAsync<Failure, IPersistenceContext> ReadUsersToRemindAboutDebtContext(
        OrderingOptions options, Instant timestamp, IPersistenceContext context) =>
        ReadUsersToRemindAboutDebtContext(context, timestamp.Minus(options.RemindUsersAboutDebtInterval));

    static EitherAsync<Failure, IPersistenceContext> ReadUsersToRemindAboutDebtContext(IPersistenceContext context, Instant previousReminder) =>
        MapReadError(
            context.ReadItems(context.Query<User>().Where(user => !user.Flags.HasFlag(UserFlags.IsDeleted) && user.LatestDebtReminder <= previousReminder)));

    public static IPersistenceContext UpdateLatestDebtReminders(IPersistenceContext context, Instant timestamp) =>
        context.UpdateItems<User>(user => user with { LatestDebtReminder = timestamp });
}
