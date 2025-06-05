using Frederikskaj2.Reservations.Users;
using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

record UpdateResidentOrderOutput(User User, Order Order, Option<Transaction> TransactionOption, bool IsCancellationWithoutFeeAllowed);