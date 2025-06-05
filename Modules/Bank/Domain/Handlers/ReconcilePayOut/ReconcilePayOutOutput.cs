using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Bank;

record ReconcilePayOutOutput(BankTransaction BankTransaction, User User, Transaction Transaction, Amount Amount);
