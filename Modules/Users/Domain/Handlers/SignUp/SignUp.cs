using Frederikskaj2.Reservations.Core;
using LanguageExt;

namespace Frederikskaj2.Reservations.Users;

static class SignUp
{
    public static SignUpOutput SignUpCore(IPasswordHasher passwordHasher, SignUpInput input) =>
        CreateOutput(passwordHasher, input, EmailAddress.NormalizeEmail(input.Command.Email));

    static SignUpOutput CreateOutput(IPasswordHasher passwordHasher, SignUpInput input, string normalizedEmail) =>
        new(
            input.Command.Timestamp,
            new(normalizedEmail, input.UserId),
            CreateUser(passwordHasher, input.Command, input.UserId, normalizedEmail));

    static User CreateUser(IPasswordHasher passwordHasher, SignUpCommand command, UserId userId, string normalizedEmail) =>
        new(
            userId,
            new EmailStatus(command.Email, normalizedEmail, IsConfirmed: false).Cons(),
            command.FullName,
            command.Phone,
            command.ApartmentId,
            new() { HashedPassword = passwordHasher.HashPassword(command.Password) },
            Roles.Resident)
        {
            Audits = UserAudit.SignUp(command.Timestamp, userId).Cons(),
        };
}
