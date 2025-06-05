using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Orders;

public record GetMyTransactionsResponse(IEnumerable<MyTransactionDto> Transactions, PaymentInformation? Payment);
