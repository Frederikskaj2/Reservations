using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Bank;

record ReconcilePayOutInput(ReconcilePayOutCommand Command, BankTransaction BankTransaction, User User, TransactionId TransactionId);