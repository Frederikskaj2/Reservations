using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Bank;

public record GetBankAccountsResponse(IEnumerable<string> BankAccounts);
