using Frederikskaj2.Reservations.Core;
using LanguageExt;
using System.Net;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

static class UpdateMyUser
{
    public static Either<Failure<Unit>, UpdateMyUserOutput> UpdateMyUserCore(UpdateMyUserInput input) =>
        from _ in CheckUserIsNotResidentWhenChangingEmailSubscriptions(input.Command.EmailSubscriptions, input.User)
        select new UpdateMyUserOutput(UpdateUser(input.Command, input.User));

    static Either<Failure<Unit>, Unit> CheckUserIsNotResidentWhenChangingEmailSubscriptions(EmailSubscriptions subscriptions, User user) =>
        user.Roles is not Roles.Resident || user.EmailSubscriptions == subscriptions
            ? unit
            : Failure.New(HttpStatusCode.Forbidden, "Residents cannot change email subscriptions.");

    static User UpdateUser(UpdateMyUserCommand command, User user) =>
        user
            .UpdateFullName(command.Timestamp, command.FullName, user.UserId)
            .UpdatePhone(command.Timestamp, command.Phone, user.UserId)
            .UpdateEmailSubscriptions(command.Timestamp, command.EmailSubscriptions);
}
