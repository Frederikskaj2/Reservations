using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared.Core;

public record MyTransactions(IEnumerable<MyTransaction> Transactions, PaymentInformation? Payment);
