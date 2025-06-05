namespace Frederikskaj2.Reservations.Bank;

public record UpdateBankTransactionCommand(BankTransactionId BankTransactionId, BankTransactionStatus Status);
