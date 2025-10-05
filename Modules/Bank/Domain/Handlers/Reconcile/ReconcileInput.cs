using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

record ReconcileInput(
    ReconcileCommand Command,
    BankTransaction BankTransaction,
    User User,
    TransactionId TransactionId,
    LocalDate LatestBankImportDate,
    Seq<ETaggedEntity<PayOut>> PayOutEntities);
