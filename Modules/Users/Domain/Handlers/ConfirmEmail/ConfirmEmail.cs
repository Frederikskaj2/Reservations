using Frederikskaj2.Reservations.Core;
using LanguageExt;
using NodaTime;
using System.Collections.Immutable;
using System.Net;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

static class ConfirmEmail
{
    public static Either<Failure<ConfirmEmailError>, ConfirmEmailOutput> ConfirmEmailCore(ITokenValidator tokenValidator, ConfirmEmailInput input) =>
        from _ in ParseToken(tokenValidator, input.Command.Timestamp, input.Command.Token, input.User.UserId)
        select new ConfirmEmailOutput(ConfirmUserEmail(input.Command.Timestamp, input.Command.Email, input.User));

    static Either<Failure<ConfirmEmailError>, Unit> ParseToken(ITokenValidator tokenValidator, Instant timestamp, ImmutableArray<byte> token, UserId userId) =>
        tokenValidator.ValidateConfirmEmailToken(timestamp, userId, token) switch
        {
            TokenValidationResult.Valid => unit,
            TokenValidationResult.Expired => Failure.New(HttpStatusCode.UnprocessableEntity, ConfirmEmailError.TokenExpired, "Token is expired."),
            _ => Failure.New(HttpStatusCode.UnprocessableEntity, ConfirmEmailError.InvalidRequest, "Token is invalid."),
        };

    static User ConfirmUserEmail(Instant timestamp, EmailAddress email, User user) =>
        user with
        {
            Emails = ConfirmUserEmail(email, user.Emails),
            Audits = user.Audits.Add(UserAudit.ConfirmEmail(timestamp, user.UserId)),
        };

    static Seq<EmailStatus> ConfirmUserEmail(EmailAddress email, Seq<EmailStatus> emails)
    {
        var emailStatus = emails.Single(status => status.NormalizedEmail == EmailAddress.NormalizeEmail(email));
        return emails.Map(status => status == emailStatus ? emailStatus with { IsConfirmed = true } : status).ToSeq();
    }
}
