using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Bank;

record ReconcilePayOutInput(ReconcilePayOutCommand Command, PayOut PayOut, BankTransaction BankTransaction, User Resident, TransactionId TransactionId);
