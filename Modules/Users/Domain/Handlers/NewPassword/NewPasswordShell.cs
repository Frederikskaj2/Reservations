using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Net;
using System.Threading;
using static Frederikskaj2.Reservations.Users.NewPassword;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

public static class NewPasswordShell
{
    public static EitherAsync<Failure<NewPasswordError>, Unit> NewPassword(
        IPasswordHasher passwordHasher,
        IPasswordValidator passwordValidator,
        IEntityReader reader,
        ITokenValidator tokenValidator,
        IEntityWriter writer,
        NewPasswordCommand command,
        CancellationToken cancellationToken) =>
        from _1 in ValidateNewPassword(passwordValidator, command.Email, command.NewPassword)
        from userEmail in reader.Read<UserEmail>(command.Email, cancellationToken).NotFoundToUnprocessableEntity().MapReadError<NewPasswordError, UserEmail>()
        from userEntity in reader.ReadWithETag<User>(userEmail.UserId, cancellationToken)
            .NotFoundToUnprocessableEntity().MapReadError<NewPasswordError, ETaggedEntity<User>>()
        from output in NewPasswordCore(passwordHasher, tokenValidator, new(command, userEntity.Value)).ToAsync()
        from _2 in writer.Write(collector => collector.Add(userEntity), tracker => tracker.Update(output.User), cancellationToken)
            .Map(_ => unit)
            .MapWriteError<NewPasswordError>()
        select unit;

    static EitherAsync<Failure<NewPasswordError>, Unit> ValidateNewPassword(IPasswordValidator validator, EmailAddress email, string password) =>
        validator.Validate(email, password).ToEitherAsync(PasswordValidationErrorToNewPasswordErrorOrSuccess);

    static Either<Failure<NewPasswordError>, Unit> PasswordValidationErrorToNewPasswordErrorOrSuccess(PasswordValidationError error) =>
        error switch
        {
            PasswordValidationError.TooShort => Failure.New(HttpStatusCode.UnprocessableEntity, NewPasswordError.TooShortPassword),
            PasswordValidationError.Exposed => Failure.New(HttpStatusCode.UnprocessableEntity, NewPasswordError.ExposedPassword),
            PasswordValidationError.SameAsEmail => Failure.New(HttpStatusCode.UnprocessableEntity, NewPasswordError.EmailSameAsPassword),
            _ => unit,
        };
}
