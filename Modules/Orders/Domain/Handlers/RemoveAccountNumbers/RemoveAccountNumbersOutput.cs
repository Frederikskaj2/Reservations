using Frederikskaj2.Reservations.Users;
using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

record RemoveAccountNumbersOutput(Seq<User> UpdatedUsers);