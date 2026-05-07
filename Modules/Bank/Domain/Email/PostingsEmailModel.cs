using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record PostingsEmailModel(EmailAddress Email, string FullName, LocalDate FromMonth, LocalDate ToMonth, Seq<Posting> Postings);
