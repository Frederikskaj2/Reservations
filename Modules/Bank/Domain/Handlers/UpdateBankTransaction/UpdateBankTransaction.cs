namespace Frederikskaj2.Reservations.Bank;

static class UpdateBankTransaction
{
    public static UpdateBankTransactionOutput UpdateBankTransactionCore(UpdateBankTransactionInput input) =>
        input.Command.Status != input.Transaction.Status
            ? new(input.Transaction with { Status = input.Command.Status }, IsChanged: true)
            : new(input.Transaction, IsChanged: false);
}
