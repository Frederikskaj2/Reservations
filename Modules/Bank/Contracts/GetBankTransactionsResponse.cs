using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Bank;

public record GetBankTransactionsResponse(IEnumerable<BankTransactionDto> Transactions);
