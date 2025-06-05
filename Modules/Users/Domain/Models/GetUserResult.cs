using LanguageExt;

namespace Frederikskaj2.Reservations.Users;

public record GetUserResult(User User, HashMap<UserId, string> UserFullNames);
