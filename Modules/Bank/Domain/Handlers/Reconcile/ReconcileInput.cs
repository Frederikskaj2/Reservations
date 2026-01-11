using Frederikskaj2.Reservations.Users;
using LanguageExt;

namespace Frederikskaj2.Reservations.Bank;

record ReconcileInput(
    ReconcileCommand Command,
    BankTransaction BankTransaction,
    User Resident,
    TransactionId TransactionId,
    Option<PayOutPair> ResidentPayOutPairOption);
