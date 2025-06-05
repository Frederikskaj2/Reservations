using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Net;
using System.Threading;
using static Frederikskaj2.Reservations.Users.ConfirmEmail;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

public static class ConfirmEmailShell
{
    public static EitherAsync<Failure<ConfirmEmailError>, Unit> ConfirmEmail(
        IEntityReader reader, ITokenValidator tokenValidator, IEntityWriter writer, ConfirmEmailCommand command, CancellationToken cancellationToken) =>
        from userEmailEntity in reader.ReadWithETag<UserEmail>(command.Email, cancellationToken).MapLeft(HideNotFoundStatus)
        from userEntity in reader.ReadWithETag<User>(userEmailEntity.Value.UserId, cancellationToken).MapLeft(HideNotFoundStatus)
        from output in ConfirmEmailCore(tokenValidator, new(command, userEntity.Value)).ToAsync()
        from _ in writer.Write(collector => collector.Add(userEntity), tracker => tracker.Update(output.User), cancellationToken)
            .Map(_ => unit)
            .MapWriteError<ConfirmEmailError>()
        select unit;

    static Failure<ConfirmEmailError> HideNotFoundStatus(HttpStatusCode status) =>
        Failure.New(MapStatusHideNotFound(status), ConfirmEmailError.Unknown, $"Database read error {status}.");

    static HttpStatusCode MapStatusHideNotFound(HttpStatusCode status) =>
        status switch
        {
            HttpStatusCode.NotFound => HttpStatusCode.UnprocessableEntity,
            _ => HttpStatusCode.ServiceUnavailable,
        };
}
