using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Bank;

record ReconcileOutput(BankTransaction BankTransaction, User User, Transaction Transaction);
