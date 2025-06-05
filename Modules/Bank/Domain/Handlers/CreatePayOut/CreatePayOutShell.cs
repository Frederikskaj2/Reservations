using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Net;
using System.Threading;
using static Frederikskaj2.Reservations.Persistence.IdGenerator;
using static Frederikskaj2.Reservations.Users.UsersFunctions;

namespace Frederikskaj2.Reservations.Bank;

public static class CreatePayOutShell
{
    public static EitherAsync<Failure<Unit>, PayOutDetails> CreatePayOut(
        IEntityReader reader, IEntityWriter writer, CreatePayOutCommand command, CancellationToken cancellationToken) =>
        from userExcerptOption in ReadUserExcerpt(reader, command.ResidentId, cancellationToken)
        from userExcerpt in GetUserExcerpt(command.ResidentId, userExcerptOption).ToAsync()
        from id in CreateId(reader, writer, nameof(PayOut), cancellationToken)
        let payOut = new PayOut(id, command.Timestamp, command.ResidentId, command.Amount)
        from tuples in writer.Write(tracker => tracker.Add(payOut), cancellationToken).MapWriteError()
        select new PayOutDetails(payOut, tuples[0].ETag, userExcerpt);

    static Either<Failure<Unit>, UserExcerpt> GetUserExcerpt(UserId userId, Option<UserExcerpt> userExcerptOption) =>
        userExcerptOption.Case switch
        {
            UserExcerpt userExcerpt => userExcerpt,
            _ => Failure.New(HttpStatusCode.NotFound, $"User with ID {userId} does not exist."),
        };
}
