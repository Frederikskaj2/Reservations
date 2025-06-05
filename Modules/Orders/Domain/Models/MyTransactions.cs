using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

public record MyTransactions(Seq<Transaction> Transactions, Option<PaymentInformation> Payment);
