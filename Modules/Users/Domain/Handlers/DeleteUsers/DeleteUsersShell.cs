using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Persistence.QueryFactory;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

public static class DeleteUsersShell
{
    static readonly IQuery<User> usersPendingDelete = Query<User>().Where(user => user.Flags.HasFlag(UserFlags.IsPendingDelete));

    public static EitherAsync<Failure<Unit>, Unit> DeleteUsers(
        IUsersEmailService emailService,
        IEntityReader reader,
        IEntityWriter writer,
        DeleteUsersCommand command,
        CancellationToken cancellationToken) =>
        from userEntities in reader.QueryWithETag(usersPendingDelete, cancellationToken).MapReadError()
        let userEntitiesToDelete = userEntities.Filter(entity => ShouldDeleteUser(entity.Value))
        from _ in DeleteUsersIfAny(emailService, reader, writer, command, userEntitiesToDelete, cancellationToken)
        select unit;

    static bool ShouldDeleteUser(User user) =>
        user.Flags.HasFlag(UserFlags.IsPendingDelete) && user.Orders.IsEmpty && user.Balance() == Amount.Zero;

    static EitherAsync<Failure<Unit>, Unit> DeleteUsersIfAny(
        IUsersEmailService emailService,
        IEntityReader reader,
        IEntityWriter writer,
        DeleteUsersCommand command,
        Seq<ETaggedEntity<User>> userEntities,
        CancellationToken cancellationToken) =>
        !userEntities.IsEmpty
            ? DeleteUsers(emailService, reader, writer, command, userEntities, cancellationToken)
            : unit;

    static EitherAsync<Failure<Unit>, Unit> DeleteUsers(
        IUsersEmailService emailService,
        IEntityReader reader,
        IEntityWriter writer,
        DeleteUsersCommand command,
        Seq<ETaggedEntity<User>> userEntities,
        CancellationToken cancellationToken) =>
        from userEmailEntities in ReadUserEmailEntities(reader, userEntities, cancellationToken)
        let output = Users.DeleteUsers.DeleteUsersCore(new(command, userEntities.ToValues()))
        from _1 in writer.Write(
            collector => collector.Add(userEntities).Add(userEmailEntities),
            tracker => tracker.Update(output.DeletedUsers.Map(deletedUser => deletedUser.User)).Remove(userEmailEntities),
            cancellationToken).MapWriteError()
        from _2 in SendUserDeletedEmails(emailService, output.DeletedUsers, cancellationToken)
        select unit;

    static EitherAsync<Failure<Unit>, Seq<ETaggedEntity<UserEmail>>> ReadUserEmailEntities(
        IEntityReader reader, Seq<ETaggedEntity<User>> userEntities, CancellationToken cancellationToken) =>
        ReadUserEmailEntities(reader, toHashSet(userEntities.Map(entity => EmailAddress.NormalizeEmail(entity.Value.Email()))), cancellationToken);

    static EitherAsync<Failure<Unit>, Seq<ETaggedEntity<UserEmail>>> ReadUserEmailEntities(
        IEntityReader reader, HashSet<string> emailAddresses, CancellationToken cancellationToken) =>
        ReadUserEmailEntities(reader, Query<UserEmail>().Where(userEmail => emailAddresses.Contains(userEmail.NormalizedEmail)), cancellationToken);

    static EitherAsync<Failure<Unit>, Seq<ETaggedEntity<UserEmail>>> ReadUserEmailEntities(
        IEntityReader reader, IQuery<UserEmail> query, CancellationToken cancellationToken) =>
        reader.QueryWithETag(query, cancellationToken).MapReadError();

    static EitherAsync<Failure<Unit>, Unit> SendUserDeletedEmails(
        IUsersEmailService emailService, Seq<DeletedUser> deletedUsers, CancellationToken cancellationToken) =>
        from _ in deletedUsers.Map(deletedUser => SendUserDeletedEmail(emailService, deletedUser, cancellationToken)).TraverseSerial(identity)
        select unit;

    static EitherAsync<Failure<Unit>, Unit> SendUserDeletedEmail(IUsersEmailService emailService, DeletedUser deletedUser, CancellationToken cancellationToken) =>
        emailService.Send(new UserDeletedEmailModel(deletedUser.EmailToDelete, deletedUser.FullName), cancellationToken);
}
