using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Bank;

public record PayOutEmailModel(EmailAddress Email, string FullName, Amount Amount);