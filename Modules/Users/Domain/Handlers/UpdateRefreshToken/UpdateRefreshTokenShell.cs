using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Users.Authentication;
using static Frederikskaj2.Reservations.Users.UpdateRefreshToken;

namespace Frederikskaj2.Reservations.Users;

public static class UpdateRefreshTokenShell
{
    public static EitherAsync<Failure<Unit>, AuthenticatedUser> UpdateRefreshToken(
        AuthenticationOptions options, IEntityReader reader, IEntityWriter writer, UpdateRefreshTokenCommand command, CancellationToken cancellationToken) =>
        from userEntity in reader.ReadWithETag<User>(command.UserId, cancellationToken).MapReadError()
        from output in UpdateRefreshTokenCore(options, new(command, userEntity.Value)).ToAsync()
        from _ in writer.Write(collector => collector.Add(userEntity), tracker => tracker.Update(output.User), cancellationToken).MapWriteError()
        select CreateAuthenticatedUser(command.Timestamp, output.User, output.RefreshToken);
}
