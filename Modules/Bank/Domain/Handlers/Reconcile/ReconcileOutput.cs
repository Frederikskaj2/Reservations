using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using LanguageExt;

namespace Frederikskaj2.Reservations.Bank;

record ReconcileOutput(BankTransaction BankTransaction, User Resident, Transaction Transaction, Option<PayOutToReconcile> PayOutToReconcile);
