using Frederikskaj2.Reservations.Users;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Orders;

public record GetUserTransactionsResponse(UserId UserId, string FullName, PaymentId PaymentId, IEnumerable<TransactionDto> Transactions);
