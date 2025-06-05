using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Bank;

record ReconcileInput(ReconcileCommand Command, BankTransaction BankTransaction, User User, TransactionId TransactionId);
