using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared.Core;

public record UserTransactions(UserId UserId, string FullName, IEnumerable<UserTransaction> Transactions);
