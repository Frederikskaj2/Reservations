using Frederikskaj2.Reservations.Core;
using LanguageExt;

namespace Frederikskaj2.Reservations.Bank;

public interface IBankTransactionsParser
{
    Either<Failure<ImportError>, Seq<ImportBankTransaction>> ParseBankTransactions(string transactions);
}
