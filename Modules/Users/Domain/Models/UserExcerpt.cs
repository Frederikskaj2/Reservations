using LanguageExt;

namespace Frederikskaj2.Reservations.Users;

public record UserExcerpt(
    UserId UserId,
    Seq<EmailStatus> Emails,
    string FullName,
    string Phone,
    Option<ApartmentId> ApartmentId,
    UserFlags Flags)
{
    public EmailAddress Email() => !Flags.HasFlag(UserFlags.IsDeleted) ? Emails.Head.Email : EmailAddress.Deleted;
}
