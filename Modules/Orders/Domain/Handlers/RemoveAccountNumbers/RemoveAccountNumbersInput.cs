using Frederikskaj2.Reservations.Users;
using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

record RemoveAccountNumbersInput(RemoveAccountNumbersCommand Command, Seq<User> Users);