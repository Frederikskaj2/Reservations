using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

record CompareBankTransaction(LocalDate Date, string Text, Amount Amount, Amount Balance);
