using NodaTime;

namespace Frederikskaj2.Reservations.Users;

static class ResendConfirmEmailEmail
{
    public static ResendConfirmEmailEmailOutput ResendConfirmEmailEmailCore(ResendConfirmEmailEmailInput input) =>
        new(UpdateUser(input.Command.Timestamp, input.User));

    static User UpdateUser(Instant timestamp, User user) =>
        user with { Audits = user.Audits.Add(UserAudit.RequestResendConfirmEmail(timestamp, user.UserId)) };
}
