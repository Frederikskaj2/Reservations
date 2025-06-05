using LanguageExt;

namespace Frederikskaj2.Reservations.Users;

public record UpdateUserResult(User User, HashMap<UserId, string> UserFullNames);
