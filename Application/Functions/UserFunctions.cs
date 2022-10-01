using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using System.Net;
using static Frederikskaj2.Reservations.Application.EmailFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

static class UserFunctions
{
    const string deletedFullName = "Slettet";
    const string deletedPhone = "Slettet";

    public static EitherAsync<Failure, IPersistenceContext> TryDeleteUser(
        IEmailService emailService, IPersistenceContext context, Instant timestamp, UserId updatedByUserId) =>
        TryDeleteUser(emailService, context, timestamp, updatedByUserId, context.Item<User>());

    static EitherAsync<Failure, IPersistenceContext> TryDeleteUser(
        IEmailService emailService, IPersistenceContext context, Instant timestamp, UserId updatedByUserId, User user) =>
        user.Flags.HasFlag(UserFlags.IsPendingDelete) && user.Orders.IsEmpty && user.Balance() == Amount.Zero
            ? DeleteUser(emailService, context, timestamp, updatedByUserId)
            : RightAsync<Failure, IPersistenceContext>(context);

    static EitherAsync<Failure, IPersistenceContext> DeleteUser(
        IEmailService emailService, IPersistenceContext context, Instant timestamp, UserId updatedByUserId) =>
        DeleteUser(emailService, context, timestamp, updatedByUserId, context.Item<User>());

    static EitherAsync<Failure, IPersistenceContext> DeleteUser(
        IEmailService emailService, IPersistenceContext context, Instant timestamp, UserId updatedByUserId, User user) =>
        from context1 in CleanupDeletedUser(context, user)
        from _ in SendUserDeletedEmail(emailService, user)
        let context2 = context1.UpdateItem<User>(User.GetId(context1.Item<User>().UserId), u => DeleteUser(u, timestamp, updatedByUserId))
        select context2;

    static User DeleteUser(User user, Instant timestamp, UserId updatedByUserId) =>
        user with
        {
            Emails = Empty,
            FullName = deletedFullName,
            Phone = deletedPhone,
            ApartmentId = Apartment.Deleted.ApartmentId,
            Security = new UserSecurity(),
            Roles = default,
            Flags = UserFlags.IsDeleted,
            EmailSubscriptions = EmailSubscriptions.None,
            FailedSign = null,
            Orders = Empty,
            Audits = user.Audits.Add(new UserAudit(timestamp, updatedByUserId, UserAuditType.Delete))
        };

    static EitherAsync<Failure, IPersistenceContext> CleanupDeletedUser(IPersistenceContext context, User user) =>
        // Notice that this doesn't handle more than a single email address.
        from context1 in MapReadError(context.ReadItem<UserEmail>(user.Emails[0].NormalizedEmail))
        select context1.DeleteItem<UserEmail>();

    static EitherAsync<Failure, IPersistenceContext> MapReadError(EitherAsync<HttpStatusCode, IPersistenceContext> either) =>
        either.MapLeft(status => Failure.New(HttpStatusCode.ServiceUnavailable, $"Database read error {status}."));

    public static AuthenticatedUser CreateAuthenticatedUser(Instant timestamp, User user, RefreshToken refreshToken) =>
        new(timestamp, user.UserId, user.Email(), user.FullName, user.Roles, refreshToken);

    public static User UpdateUser(User user, UpdateMyUserCommand command) =>
        user
            .UpdateFullName(command.Timestamp, command.FullName, user.UserId)
            .UpdatePhone(command.Timestamp, command.Phone, user.UserId)
            .UpdateEmailSubscriptions(command.Timestamp, command.EmailSubscriptions);

    public static User UpdateUser(User user, UpdateUserCommand command) =>
        user
            .UpdateFullName(command.Timestamp, command.FullName, command.AdministratorUserId)
            .UpdatePhone(command.Timestamp, command.Phone, command.AdministratorUserId);

    public static EitherAsync<Failure, User> TryUpdateRoles(User user, UpdateUserCommand command) =>
        user.Roles != command.Roles && user.Roles.HasFlag(Roles.UserAdministration) && !command.Roles.HasFlag(Roles.UserAdministration) &&
        user.UserId == command.AdministratorUserId
            ? Failure.New(HttpStatusCode.UnprocessableEntity, "User cannot remove user administration role from self.")
            : user.UpdateRoles(command.Timestamp, command.Roles, command.AdministratorUserId);

    public static EitherAsync<Failure, User> TryUpdateIsPendingDelete(User user, UpdateUserCommand command) =>
        !user.Flags.HasFlag(UserFlags.IsPendingDelete) && command.IsPendingDelete && user.UserId == command.AdministratorUserId
            ? Failure.New(HttpStatusCode.UnprocessableEntity, "User in role user administration cannot delete self.")
            : user.UpdateIsPendingDelete(command.Timestamp, command.IsPendingDelete, command.AdministratorUserId);

    public static EitherAsync<Failure, User> TryUpdateIsPendingDelete(User user, DeleteMyUserCommand command) =>
        !user.Flags.HasFlag(UserFlags.IsPendingDelete) && user.Roles.HasFlag(Roles.UserAdministration)
            ? Failure.New(HttpStatusCode.UnprocessableEntity, "User in role user administration cannot delete self.")
            : user.UpdateIsPendingDelete(command.Timestamp, true, user.UserId);

    public static User TryRemoveAccountNumber(Instant timestamp, UserId updatedByUserId, User user) =>
        user.Orders.IsEmpty && user.Balance() == Amount.Zero
            ? user.RemoveAccountNumber(timestamp, updatedByUserId)
            : user;
}
