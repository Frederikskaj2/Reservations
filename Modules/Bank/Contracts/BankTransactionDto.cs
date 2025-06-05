using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record BankTransactionDto(BankTransactionId BankTransactionId, LocalDate Date, string Text, Amount Amount, Amount Balance, BankTransactionStatus Status);
