using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using NodaTime;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Users.SignUp;
using static Frederikskaj2.Reservations.Users.UsersFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

public static class SignUpShell
{
    public static EitherAsync<Failure<SignUpError>, Unit> SignUp(
        IUsersEmailService emailService,
        IPasswordHasher passwordHasher,
        IPasswordValidator passwordValidator,
        IEntityReader reader,
        IEntityWriter writer,
        SignUpCommand command,
        CancellationToken cancellationToken) =>
        from input in CreateInput(passwordValidator, reader, writer, command, cancellationToken)
        let output = SignUpCore(passwordHasher, input)
        from newOrExisting in WriteWithConflictHandling(reader, writer, output, cancellationToken)
        from _ in SendEmail(emailService, output, newOrExisting, cancellationToken)
        select unit;

    static EitherAsync<Failure<SignUpError>, SignUpInput> CreateInput(
        IPasswordValidator passwordValidator,
        IEntityReader reader,
        IEntityWriter writer,
        SignUpCommand command,
        CancellationToken cancellationToken) =>
        from _1 in ValidatePassword(passwordValidator, command)
        from userId in CreateId(reader, writer, cancellationToken)
        select new SignUpInput(command, userId);

    static EitherAsync<Failure<SignUpError>, Unit> ValidatePassword(IPasswordValidator validator, SignUpCommand command) =>
        validator.Validate(command.Email, command.Password).ToEitherAsync(PasswordValidationErrorToFailure);

    static Either<Failure<SignUpError>, Unit> PasswordValidationErrorToFailure(PasswordValidationError error) =>
        error switch
        {
            PasswordValidationError.TooShort => Failure.New(HttpStatusCode.UnprocessableEntity, SignUpError.TooShortPassword),
            PasswordValidationError.Exposed => Failure.New(HttpStatusCode.UnprocessableEntity, SignUpError.ExposedPassword),
            PasswordValidationError.SameAsEmail => Failure.New(HttpStatusCode.UnprocessableEntity, SignUpError.EmailSameAsPassword),
            _ => unit,
        };

    static EitherAsync<Failure<SignUpError>, int> CreateId(IEntityReader reader, IEntityWriter writer, CancellationToken cancellationToken) =>
        IdGenerator.CreateId(reader, writer, nameof(User), cancellationToken)
            .MapLeft(failure => Failure.New(HttpStatusCode.ServiceUnavailable, SignUpError.Unknown, failure.Detail));

    static EitherAsync<Failure<SignUpError>, NewOrExisting> WriteWithConflictHandling(
        IEntityReader reader, IEntityWriter writer, SignUpOutput output, CancellationToken cancellationToken) =>
        writer.Write(tracker => tracker.Add(output.UserEmail).Add(output.User), cancellationToken)
            .Map(_ => unit)
            .MapWriteError<SignUpError>()
            .BiBind(
                _ => NewOrExisting.New,
                failure => ReadExistingUserOnConflict(reader, writer, output, failure, cancellationToken));

    static EitherAsync<Failure<SignUpError>, NewOrExisting> ReadExistingUserOnConflict(
        IEntityReader reader, IEntityWriter writer, SignUpOutput output, Failure<SignUpError> failure, CancellationToken cancellationToken) =>
        failure.Status switch
        {
            HttpStatusCode.Conflict => ReadExistingUser(reader, writer, output.Timestamp, output.User.Email(), cancellationToken),
            _ => failure,
        };

    static EitherAsync<Failure<SignUpError>, NewOrExisting> ReadExistingUser(
        IEntityReader reader, IEntityWriter writer, Instant timestamp, EmailAddress email, CancellationToken cancellationToken) =>
        from _ in ReadUserWithRequestNewPasswordAudit<SignUpError>(reader, writer, timestamp, email, cancellationToken)
        select NewOrExisting.Existing;

    static EitherAsync<Failure<SignUpError>, Unit> SendEmail(
        IUsersEmailService emailService, SignUpOutput output, NewOrExisting newOrExisting, CancellationToken cancellationToken) =>
        SendEmail(emailService, output.Timestamp, output.User, newOrExisting, cancellationToken).ToRightAsync<Failure<SignUpError>, Unit>();

    static Task<Unit> SendEmail(
        IUsersEmailService emailService, Instant timestamp, User user, NewOrExisting newOrExisting, CancellationToken cancellationToken) =>
        newOrExisting switch
        {
            NewOrExisting.New => emailService.Send(new ConfirmEmailEmailModel(user.Created(), user.UserId, user.Email(), user.FullName), cancellationToken),
            NewOrExisting.Existing => emailService.Send(new NewPasswordEmailModel(timestamp, user.Email(), user.FullName), cancellationToken),
            _ => throw new UnreachableException(),
        };

    enum NewOrExisting
    {
        New,
        Existing,
    }
}
