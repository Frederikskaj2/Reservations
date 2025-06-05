using Frederikskaj2.Reservations.Users;
using LanguageExt;

namespace Frederikskaj2.Reservations.Bank;

public record PostingsForMonth(Seq<Posting> Postings, HashMap<UserId, UserExcerpt> Users);
