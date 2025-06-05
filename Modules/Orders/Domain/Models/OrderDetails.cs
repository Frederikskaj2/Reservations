using Frederikskaj2.Reservations.Users;
using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

public record OrderDetails(Order Order, User User, HashMap<UserId, string> AuditsUserFullNames);
