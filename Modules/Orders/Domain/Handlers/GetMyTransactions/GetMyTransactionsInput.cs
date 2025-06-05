using Frederikskaj2.Reservations.Users;
using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

record GetMyTransactionsInput(User User, Seq<Transaction> Transactions);