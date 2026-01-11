using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Bank;

record ReconcilePayOutOutput(PayOut PayOut, BankTransaction BankTransaction, User Resident, Transaction Transaction, Amount Amount);
