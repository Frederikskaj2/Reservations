using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;

namespace Frederikskaj2.Reservations.Application;

static class DebtReminderFunctions
{
    public static IPersistenceContext TrySetLatestDebtReminder(IPersistenceContext context, Instant timestamp) =>
        context.UpdateItem<User>(user => TrySetLatestDebtReminder(user, timestamp));

    static User TrySetLatestDebtReminder(User user, Instant timestamp) =>
        user.Balance() > Amount.Zero && !user.LatestDebtReminder.HasValue
            ? user with { LatestDebtReminder = timestamp }
            : user;

    public static IPersistenceContext TryClearLatestDebtReminder(IPersistenceContext context) =>
        context.UpdateItem<User>(TryClearLatestDebtReminder);

    static User TryClearLatestDebtReminder(User user) =>
        user.Balance() <= Amount.Zero && user.LatestDebtReminder.HasValue
            ? user with { LatestDebtReminder = null }
            : user;

    public static EitherAsync<Failure, IPersistenceContext> ReadUsersToRemindAboutDebtContext(
        OrderingOptions options, Instant timestamp, IPersistenceContext context) =>
        ReadUsersToRemindAboutDebtContext(context, timestamp.Minus(options.RemindUsersAboutDebtInterval));

    static EitherAsync<Failure, IPersistenceContext> ReadUsersToRemindAboutDebtContext(IPersistenceContext context, Instant previousReminder) =>
        MapReadError(context.ReadItems(context.Query<User>().Where(user => !user.Flags.HasFlag(UserFlags.IsDeleted) && user.LatestDebtReminder <= previousReminder)));

    public static IPersistenceContext UpdateLatestDebtReminders(IPersistenceContext context, Instant timestamp) =>
        context.UpdateItems<User>(user => user with { LatestDebtReminder = timestamp });
}
