namespace Frederikskaj2.Reservations.Orders;

static class GetMyTransactions
{
    public static GetMyTransactionsOutput GetMyTransactionsCore(OrderingOptions options, GetMyTransactionsInput input) =>
        new(new(input.Transactions, PaymentFunctions.GetPaymentInformation(options, input.User)));
}
