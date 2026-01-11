using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Bank.PostingFunctions;
using static Frederikskaj2.Reservations.Users.UsersFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Bank;

public static class SendPostingsShell
{
    public static EitherAsync<Failure<Unit>, EmailAddress> SendPostings(
        IBankEmailService emailService, IEntityReader reader, SendPostingsCommand command, CancellationToken cancellationToken) =>
        from user in reader.Read<User>(command.UserId, cancellationToken).MapReadError()
        from postings in GetPostingsV1OrV2(reader, command.Month, cancellationToken).MapReadError()
        let userIds = toHashSet(postings.Map(posting => posting.ResidentId))
        from userExcerpts in ReadUserExcerpts(reader, userIds, cancellationToken)
        let userFullNames = toHashMap(userExcerpts.Map(userExcerpt => (userExcerpt.UserId, userExcerpt.FullName)))
        from _ in emailService
            .Send(new(user.Email(), user.FullName, command.Month, postings), userFullNames, cancellationToken)
            .ToRightAsync<Failure<Unit>, Unit>()
        select user.Email();
}
