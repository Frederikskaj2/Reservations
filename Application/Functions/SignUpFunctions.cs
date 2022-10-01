using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using System;
using System.Net;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.UpdatePasswordFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

static class SignUpFunctions
{
    public static EitherAsync<Failure<SignUpError>, Unit> ValidatePassword(IPasswordValidator validator, SignUpCommand command) =>
        validator.ValidateAsync(command.Email, command.Password).ToEitherAsync(PasswordValidationErrorToFailure);

    static Either<Failure<SignUpError>, Unit> PasswordValidationErrorToFailure(PasswordValidationError error) =>
        error switch
        {
            PasswordValidationError.TooShort => Failure.New(HttpStatusCode.UnprocessableEntity, SignUpError.TooShortPassword),
            PasswordValidationError.Exposed => Failure.New(HttpStatusCode.UnprocessableEntity, SignUpError.ExposedPassword),
            PasswordValidationError.SameAsEmail => Failure.New(HttpStatusCode.UnprocessableEntity, SignUpError.EmailSameAsPassword),
            _ => unit
        };

    public static EitherAsync<Failure<SignUpError>, int> CreateId(IPersistenceContextFactory contextFactory) =>
        IdGenerator.CreateId(contextFactory, nameof(User))
            .MapLeft(failure => Failure.New(HttpStatusCode.ServiceUnavailable, SignUpError.Unknown, failure.Detail));

    public static IPersistenceContext CreateUser(IPasswordHasher passwordHasher, IPersistenceContext context, SignUpCommand command, UserId userId) =>
        CreateUser(passwordHasher, context, command, userId, EmailAddress.NormalizeEmail(command.Email));

    static IPersistenceContext CreateUser(IPasswordHasher passwordHasher, IPersistenceContext context, SignUpCommand command, UserId userId, string normalizedEmail) =>
        context
            .CreateItem(
                normalizedEmail,
                new UserEmail(normalizedEmail, userId))
            .CreateItem(
                User.GetId(userId),
                CreateUser(passwordHasher, command, userId, normalizedEmail) with
                {
                    Audits = CreateUser(passwordHasher, command, userId, normalizedEmail).Audits
                        .Add(new UserAudit(command.Timestamp, CreateUser(passwordHasher, command, userId, normalizedEmail).UserId, UserAuditType.SignUp))
                });

    static User CreateUser(IPasswordHasher passwordHasher, SignUpCommand command, UserId userId, string normalizedEmail) =>
        new(
            userId,
            new EmailStatus(command.Email, normalizedEmail, false).Cons(),
            command.FullName,
            command.Phone,
            command.ApartmentId.ToNullable(),
            new UserSecurity { HashedPassword = passwordHasher.HashPassword(command.Password) },
            Roles.Resident);

    public static EitherAsync<Failure<SignUpError>, (NewOrExisting NewOrExisting, User User)> Write(IPersistenceContext context) =>
        HandleWriteError(WriteWithConflictHandling(context));

    static EitherAsync<HttpStatusCode, (NewOrExisting, User)> WriteWithConflictHandling(IPersistenceContext context) =>
        context.Write().BiBind(
            Right: _ => (NewOrExisting.New, context.Item<User>()),
            Left: status => status switch
            {
                HttpStatusCode.Conflict => ReadExistingUser(context),
                _ => status
            });

    static EitherAsync<HttpStatusCode, (NewOrExisting, User)> ReadExistingUser(IPersistenceContext context) =>
        ReadExistingUser(CreateContext(context.Factory), context.Item<User>());

    static EitherAsync<HttpStatusCode, (NewOrExisting, User)> ReadExistingUser(IPersistenceContext context, User user) =>
        ReadExistingUser(context, user.Email(), user.Audits.Last().Timestamp);

    static EitherAsync<HttpStatusCode, (NewOrExisting, User)> ReadExistingUser(IPersistenceContext context, EmailAddress email, Instant timestamp) =>
        from user in GetUserWithRequestNewPasswordAudit(context, timestamp, email).MapLeft(failure => failure.Status)
        select (NewOrExisting.Existing, user);

    static EitherAsync<Failure<SignUpError>, (NewOrExisting, User)> HandleWriteError(EitherAsync<HttpStatusCode, (NewOrExisting, User)> either) =>
        either.MapLeft(MapWriteError);

    static Failure<SignUpError> MapWriteError(HttpStatusCode status) =>
        Failure.New(HttpStatusCode.ServiceUnavailable, default(SignUpError), $"Database read error {status}.");

    public static Task<Unit> SendEmail(IEmailService emailService, Instant timestamp, User user, NewOrExisting newOrExisting) =>
        newOrExisting switch
        {
            NewOrExisting.New => emailService.Send(new ConfirmEmailEmailModel(user.Created(), user.UserId, user.Email(), user.FullName)),
            NewOrExisting.Existing => emailService.Send(new NewPasswordEmailModel(timestamp, user.Email(), user.FullName)),
            _ => throw new ArgumentOutOfRangeException(nameof(newOrExisting), newOrExisting, null)
        };
}
