using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Net;
using System.Threading;
using static Frederikskaj2.Reservations.Users.Authentication;
using static Frederikskaj2.Reservations.Users.UpdatePassword;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

public static class UpdatePasswordShell
{
    public static EitherAsync<Failure<PasswordError>, AuthenticatedUser> UpdatePassword(
        AuthenticationOptions options,
        IPasswordHasher passwordHasher,
        IPasswordValidator passwordValidator,
        IEntityReader reader,
        IEntityWriter writer,
        UpdatePasswordCommand command,
        CancellationToken cancellationToken) =>
        from userEntity in reader.ReadWithETag<User>(command.UserId, cancellationToken).NotFoundToForbidden().MapReadError<PasswordError, ETaggedEntity<User>>()
        from _1 in ValidateUpdatePassword(passwordValidator, userEntity.Value.Email(), command.NewPassword)
        from output in UpdatePasswordCore(options, passwordHasher, new(command, userEntity.Value)).ToAsync()
        from _2 in WriteUser(writer, userEntity, output, cancellationToken)
        select CreateAuthenticatedUser(command.Timestamp, output.User, output.RefreshToken);

    static EitherAsync<Failure<PasswordError>, Unit> ValidateUpdatePassword(IPasswordValidator validator, EmailAddress email, string password) =>
        validator.Validate(email, password).ToEitherAsync(PasswordValidationErrorToUpdatePasswordFailure);

    static Either<Failure<PasswordError>, Unit> PasswordValidationErrorToUpdatePasswordFailure(PasswordValidationError error) =>
        error switch
        {
            PasswordValidationError.TooShort => Failure.New(HttpStatusCode.UnprocessableEntity, PasswordError.TooShortPassword),
            PasswordValidationError.Exposed => Failure.New(HttpStatusCode.UnprocessableEntity, PasswordError.ExposedPassword),
            PasswordValidationError.SameAsEmail => Failure.New(HttpStatusCode.UnprocessableEntity, PasswordError.EmailSameAsPassword),
            _ => unit,
        };

    static EitherAsync<Failure<PasswordError>, Unit> WriteUser(IEntityWriter writer, ETaggedEntity<User> userEntity, UpdatePasswordOutput output, CancellationToken cancellationToken) =>
        writer.Write(collector => collector.Add(userEntity), tracker => tracker.Update(output.User), cancellationToken)
            .Map(_ => unit)
            .MapWriteError<PasswordError>();
}
