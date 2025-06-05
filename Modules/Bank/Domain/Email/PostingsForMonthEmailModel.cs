using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record PostingsForMonthEmailModel(EmailAddress Email, string FullName, LocalDate Month, Seq<Posting> Postings);
