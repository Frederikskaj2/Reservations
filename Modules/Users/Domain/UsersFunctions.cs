using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using NodaTime;
using System.Threading;
using static Frederikskaj2.Reservations.Persistence.QueryFactory;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

public static class UsersFunctions
{
    public static EitherAsync<Failure<Unit>, Seq<EmailUser>> ReadSubscribedEmailUsers(
        IEntityReader reader, EmailSubscriptions subscriptions, CancellationToken cancellationToken) =>
        reader.Query(GetEmailUsersQuery(subscriptions), cancellationToken).MapReadError();

    public static EitherAsync<Failure<TFailure>, User> ReadUserWithRequestNewPasswordAudit<TFailure>(
        IEntityReader reader, IEntityWriter writer, Instant timestamp, EmailAddress email, CancellationToken cancellationToken) where TFailure : struct =>
        from userEmailEntity in reader.Read<UserEmail>(email, cancellationToken).MapReadError<TFailure, UserEmail>()
        from userEntity in reader.ReadWithETag<User>(userEmailEntity.UserId, cancellationToken).MapReadError<TFailure, ETaggedEntity<User>>()
        let user = userEntity.Value with { Audits = userEntity.Value.Audits.Add(UserAudit.RequestNewPassword(timestamp, userEntity.Value.UserId)) }
        from _ in writer.Write(collector => collector.Add(userEntity), tracker => tracker.Update(user), cancellationToken)
            .Map(_ => unit)
            .MapWriteError<TFailure>()
        select user;

    static IProjectedQuery<EmailUser> GetEmailUsersQuery(EmailSubscriptions subscriptions) =>
        Query<User>()
            .Where(user => user.EmailSubscriptions.HasFlag(subscriptions))
            .Project(user => new EmailUser(user.UserId, user.Emails[0].Email, user.FullName));

    public static EitherAsync<Failure<Unit>, HashMap<UserId, string>> ReadUserFullNamesMapForUser(
        IEntityReader reader, User user, CancellationToken cancellationToken) =>
        ReadUserFullNamesMap(reader, GetUserIds(user), cancellationToken);

    static HashSet<UserId> GetUserIds(User user) =>
        toHashSet(user.Audits.Map(audit => audit.UserId).Somes());

    public static HashMap<UserId, string> CreateUserFullNamesMap(User user) =>
        HashMap((user.UserId, user.FullName));

    public static EitherAsync<Failure<Unit>, HashMap<UserId, string>> ReadUserFullNamesMap(
        IEntityReader reader, HashSet<UserId> userIds, CancellationToken cancellationToken) =>
        !userIds.IsEmpty
            ? ReadUserFullNames(reader, userIds, cancellationToken)
            : HashMap<UserId, string>();

    static EitherAsync<Failure<Unit>, HashMap<UserId, string>> ReadUserFullNames(
        IEntityReader reader, HashSet<UserId> userIds, CancellationToken cancellationToken) =>
        from userFullNames in reader.Query(GetUsersQuery(userIds), cancellationToken).MapReadError()
        select AddSystemUser(toHashMap(userFullNames.Map(userFullName => (userFullName.UserId, userFullName.FullName))));

    static IProjectedQuery<UserFullName> GetUsersQuery(HashSet<UserId> userIds) =>
        Query<User>()
            .Where(user => userIds.Contains(user.UserId))
            .Project(user => new UserFullName(user.UserId, user.FullName));

    static HashMap<UserId, string> AddSystemUser(HashMap<UserId, string> userFullNames) =>
        userFullNames.Add(UserId.System, "System");

    public static EitherAsync<Failure<Unit>, Seq<UserExcerpt>> ReadUserExcerpts(
        IEntityReader reader, HashSet<UserId> userIds, CancellationToken cancellationToken) =>
        !userIds.IsEmpty
            ? reader.Query(GetUserExcerptsQuery(userIds), cancellationToken).MapReadError()
            : Seq<UserExcerpt>();

    static IProjectedQuery<UserExcerpt> GetUserExcerptsQuery(HashSet<UserId> userIds) =>
        Query<User>()
            .Where(user => userIds.Contains(user.UserId))
            .Project(user => new UserExcerpt(user.UserId, user.Emails, user.FullName, user.Phone, user.ApartmentId, user.Flags));

    public static EitherAsync<Failure<Unit>, Option<UserExcerpt>> ReadUserExcerpt(IEntityReader reader, UserId userId, CancellationToken cancellationToken) =>
        from userExcerpts in reader.Query(GetUserExcerptQuery(userId), cancellationToken).MapReadError()
        select userExcerpts.ToOption();

    static IProjectedQuery<UserExcerpt> GetUserExcerptQuery(UserId userId) =>
        Query<User>()
            .Where(user => user.UserId == userId)
            .Project(user => new UserExcerpt(user.UserId, user.Emails, user.FullName, user.Phone, user.ApartmentId, user.Flags));
}
