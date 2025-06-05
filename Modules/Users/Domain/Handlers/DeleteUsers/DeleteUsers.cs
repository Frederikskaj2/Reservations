using NodaTime;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

static class DeleteUsers
{
    const string deletedFullName = "Slettet";
    const string deletedPhone = "Slettet";

    public static DeleteUsersOutput DeleteUsersCore(DeleteUsersInput input) =>
        new(input.Users.Map(user => DeleteUser(input.Command.Timestamp, user)));

    static DeletedUser DeleteUser(Instant timestamp, User user) =>
        new(
            user with
            {
                Emails = Empty,
                FullName = deletedFullName,
                Phone = deletedPhone,
                ApartmentId = Apartment.Deleted.ApartmentId,
                Security = new(),
                Roles = default,
                Flags = UserFlags.IsDeleted,
                AccountNumber = None,
                EmailSubscriptions = EmailSubscriptions.None,
                FailedSign = None,
                Orders = Empty,
                Audits = user.Audits.Add(UserAudit.Delete(timestamp)),
            },
            user.Email(),
            user.FullName);
}
